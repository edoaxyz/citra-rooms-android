using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using ENet;
using Be.IO;

using CitraRooms.Main;

namespace CitraRooms.Rooms
{
    class CustomBinaryWriter : BeBinaryWriter
    {
        public CustomBinaryWriter(Stream output) : base(output)
        {
        }

        public CustomBinaryWriter(Stream output, Encoding encoding) : base(output, encoding)
        {
        }

        public CustomBinaryWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
        {
        }

        public override void Write(string value)
        {
            base.Write((UInt32)value.Length);
            base.Write(Encoding.UTF8.GetBytes(value));
        }
    }

    class CustomBinaryReader : BeBinaryReader
    {
        public CustomBinaryReader(Stream input) : base(input)
        {
        }

        public CustomBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public CustomBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public override string ReadString()
        {
            long lenght = base.ReadUInt32();
            List<char> chars = new List<char>();
            lenght += this.BaseStream.Position;
            while (this.BaseStream.Position < lenght)
            {
                char c = base.ReadChar();
                chars.Add(c);
            }
            return new String(chars.ToArray());
        }
    }


    class RoomListenerException : Exception
    {
        public RoomListenerException(string message) : base(message) { }
    }

    enum State
    {
        Joining,
        Joined,
        Disconnected
    }

    enum MessageTypes : byte
    {
        IdJoinRequest = 0x01,
        IdJoinSuccess,
        IdRoomInformation,
        IdSetGameInfo,
        IdWifiPacket,
        IdChatMessage,
        IdNameCollision,
        IdMacCollision,
        IdVersionMismatch,
        IdWrongPassword,
        IdCloseRoom
    };

    struct LoginData
    {
        public readonly string Username;
        public readonly string Password;

        public LoginData(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    struct ChatMessage
    {
        public string Username;
        public string Message;
        public DateTime TimeStamp;
    }
    

    class RoomListener
    {
        private const byte netVersion = 0x03;

        private Host client;
        private Peer server;
        private LoginData loginData;

        public State state;
        public event EventHandler<Room> OnRoomUpdate;
        public event EventHandler OnConnect;
        public event EventHandler<ChatMessage> OnMessageReceived;

        public RoomListener(String address, int port, String username, String password)
        {
            ENet.Library.Initialize();
            client = new Host();
            client.Initialize(null, 1);

            server = client.Connect(address, port, 0);

            state = State.Joining;

            loginData = new LoginData(username, password);
        }

        private void TryConnect(int netVersion, Event @event)
        {
            byte[] MAC = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            using (MemoryStream ms = new MemoryStream())
            {
                using (CustomBinaryWriter bw = new CustomBinaryWriter(ms))
                {
                    bw.Write((byte)MessageTypes.IdJoinRequest);
                    bw.Write(loginData.Username);
                    bw.Write(MAC);
                    bw.Write((UInt32)netVersion);
                    bw.Write(loginData.Password);
                }
                System.Diagnostics.Debug.WriteLine(BitConverter.ToString(ms.ToArray()));
                server.Send(@event.ChannelID, ms.ToArray(), PacketFlags.Reliable);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (CustomBinaryWriter bw = new CustomBinaryWriter(ms))
                {
                    bw.Write((byte)MessageTypes.IdSetGameInfo);
                    bw.Write("Citra Rooms");
                    bw.Write((UInt64)1);
                }
                server.Send(@event.ChannelID, ms.ToArray(), PacketFlags.Reliable);
            }

        }

        public bool IsConnected()
        {
            return state == State.Joined || state == State.Joining;
        }

        public void Listen()
        {
            while (IsConnected())
            {
                Event @event;

                if (client.Service(100, out @event)) {

                    do
                    {
                        switch (@event.Type)
                        {
                            case EventType.Receive:
                                switch ((MessageTypes)@event.Packet.GetBytes()[0])
                                {
                                    case MessageTypes.IdChatMessage:
                                        HandleChatMessage(@event);
                                        break;
                                    case MessageTypes.IdRoomInformation:
                                        HandleRoomInformation(@event);
                                        break;
                                    case MessageTypes.IdJoinSuccess:
                                        state = State.Joined;
                                        OnConnect(this, null);
                                        break;
                                    case MessageTypes.IdNameCollision:
                                        state = State.Disconnected;
                                        throw new RoomListenerException("Username already taken. Please change it in settings.");
                                    case MessageTypes.IdMacCollision:
                                        state = State.Disconnected;
                                        throw new RoomListenerException("Server couldn't assign a valid MAC address.");
                                    case MessageTypes.IdVersionMismatch:
                                        HandleVersionMismatch(@event);
                                        break;
                                    case MessageTypes.IdWrongPassword:
                                        state = State.Disconnected;
                                        throw new RoomListenerException("Wrong password.");
                                    case MessageTypes.IdCloseRoom:
                                        state = State.Disconnected;
                                        throw new RoomListenerException("Server closed connection.");
                                }
                                break;
                            case EventType.Disconnect:
                                state = State.Disconnected;
                                throw new RoomListenerException("Unable to connect to server.");
                            case EventType.Connect:
                                TryConnect(netVersion, @event);
                                break;
                        }
                    }
                    while (client.CheckEvents(out @event));
                }
            }
        }

        private void HandleVersionMismatch(Event @event)
        {
            using (MemoryStream ms = new MemoryStream(@event.Packet.GetBytes()))
            {
                ms.Position = 1;
                using (CustomBinaryReader br = new CustomBinaryReader(ms))
                {
                    TryConnect(br.ReadInt32(), @event);
                }
            }
        }

        private void HandleRoomInformation(Event @event)
        {
            Room room = new Room();
            using (MemoryStream ms = new MemoryStream(@event.Packet.GetBytes()))
            {
                using (CustomBinaryReader br = new CustomBinaryReader(ms))
                {
                    br.ReadBytes(1);
                    room.name = br.ReadString();
                    room.maxPlayers = (int)br.ReadUInt32();
                    br.ReadString(); // Room UID
                    room.port = br.ReadUInt16();
                    room.preferredGameName = br.ReadString();
                    room.players = new HashSet<Player>();
                    int len = (int)br.ReadUInt32();
                    for (int i=0; i < len; i++)
                    {
                        Player p = new Player();
                        p.name = br.ReadString();
                        br.ReadBytes(6);
                        p.gameName = br.ReadString();
                        p.gameId = (long)br.ReadInt64();
                        room.players.Add(p);
                    }
                }
                OnRoomUpdate(this, room);
            }
        }

        private void HandleChatMessage(Event @event)
        {
            ChatMessage mess = new ChatMessage();
            using (MemoryStream ms = new MemoryStream(@event.Packet.GetBytes()))
            {
                ms.Position = 1;
                using (CustomBinaryReader br = new CustomBinaryReader(ms))
                {
                    mess.Username = br.ReadString();
                    mess.Message = br.ReadString();
                    mess.TimeStamp = DateTime.Now;
                }
                OnMessageReceived(this, mess);
            }
        }

        public void SendChatMessage(String message)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (CustomBinaryWriter bw = new CustomBinaryWriter(ms))
                {
                    bw.Write((byte)MessageTypes.IdChatMessage);
                    bw.Write(message);
                }
                server.Send(0, ms.ToArray(), PacketFlags.Reliable);
            }
        }

        public void CloseConnection()
        {
            server.DisconnectNow(0);
        }
    }
}