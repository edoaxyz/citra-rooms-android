using System;
using System.Timers;
using System.Net;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace CitraRooms.Main
{
    public class Room : IEquatable<Room>
    {
        public string address { get; set; }
        public int port { get; set; }
        public int netVersion { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public long preferredGameId { get; set; }
        public string preferredGameName { get; set; }
        public bool hasPassword { get; set; }
        public int maxPlayers { get; set; }
        public HashSet<Player> players { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Room);
        }

        public bool Equals(Room other)
        {
            return other != null &&
                   address == other.address &&
                   port == other.port &&
                   netVersion == other.netVersion &&
                   name == other.name &&
                   owner == other.owner &&
                   preferredGameId == other.preferredGameId &&
                   preferredGameName == other.preferredGameName &&
                   hasPassword == other.hasPassword &&
                   maxPlayers == other.maxPlayers &&
                   EqualityComparer<HashSet<Player>>.Default.Equals(players, other.players);
        }
    }

    public class Player : IEquatable<Player>
    {
        public long gameId { get; set; }
        public string gameName { get; set; }
        public string name { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Player);
        }

        public bool Equals(Player other)
        {
            return other != null &&
                name == other.name;
        }

        public override int GetHashCode()
        {
            return 363513814 + EqualityComparer<string>.Default.GetHashCode(name);
        }
    }


    public class LobbyUpdater
    {
        public event EventHandler<List<Room>> OnRoomUpdate;
        private Timer timer;
        private bool stopped;

        public LobbyUpdater(int interval)
        {
            timer = new Timer
            {
                Interval = interval
            };
            timer.Elapsed += new ElapsedEventHandler(UpdateLobby);
            stopped = false;
            timer.Start();
        }

        public void UpdateLobby(Object myObject, EventArgs myEventArgs)
        {
            try
            {
                timer.Stop();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.citra-emu.org/lobby");
                request.Method = "GET";

                var responseReader = new StreamReader(request.GetResponse().GetResponseStream());
                String response = responseReader.ReadToEnd();
                responseReader.Close();

                Dictionary<String, List<Room>> data = JsonConvert.DeserializeObject<Dictionary<String, List<Room>>>(response);

                this.OnRoomUpdate(this, data["rooms"]);
                timer.Start();
            }
            catch (Exception e)
            {
                if (!stopped) timer.Start();
            }
        }

        public void Start()
        {
            stopped = false;
            timer.Start();
        }

        public void Stop()
        {
            stopped = true;
            timer.Stop();
        }
    }
}
