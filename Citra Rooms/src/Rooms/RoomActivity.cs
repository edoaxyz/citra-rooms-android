using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;

using CitraRooms.CustomViews;
using CitraRooms.CustomViews.Chat;
using CitraRooms.Main;

namespace CitraRooms.Rooms
{
    [Activity(Label = "RoomActivity", Theme = "@android:style/Theme.Material", MainLauncher = false)]
    public class RoomActivity : Activity, IDialogInterfaceOnDismissListener
    {
        private RoomListener listener;
        private Room room = null;

        private Dictionary<String, Color> colors;
        private HashSet<Color> unusedColors;
        
        private LinearLayout messageContainer;

        private MediaPlayer newMessage;

        private bool bottomReached = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_room);
            
            ActionBar.SetDisplayHomeAsUpEnabled(true);
            ActionBar.Title = "Connecting...";

            messageContainer = (LinearLayout)FindViewById(Resource.Id.messages);
            colors = new Dictionary<String, Color>();
            unusedColors = new HashSet<Color>();
            foreach (String str in Utility.LightColors)
            {
                unusedColors.Add(Color.ParseColor(str));
            }

            listener = new RoomListener(Intent.GetStringExtra("address"), Intent.GetIntExtra("port", 0), Intent.GetStringExtra("username"), Intent.GetStringExtra("password"));
            listener.OnRoomUpdate += UpdateInfoRoom;
            listener.OnConnect += Connected;
            listener.OnMessageReceived += AddMessage;
            listener.OnStatusReceived += AddStatusMessage;

            ((ImageButton)FindViewById(Resource.Id.sendMessageButton)).Click += SendMessage;

            ((ScrollView)FindViewById(Resource.Id.scrollMessages)).SystemUiVisibilityChange += OnScroll;

            messageContainer.LayoutChange += (o, e) => { if (bottomReached) ((ScrollView)FindViewById(Resource.Id.scrollMessages)).FullScroll(FocusSearchDirection.Down); };

            newMessage = MediaPlayer.Create(this, Resource.Raw.sound_in);

            new Thread(StartListening).Start();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_room, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            System.Diagnostics.Debug.WriteLine(item.ItemId + " " + Resource.Id.infoButton);
            switch (item.ItemId)
            {
                case Resource.Id.infoButton:
                    if (room != null) new InfoRoomDialog(this,room).Show();
                    return true;
                case Android.Resource.Id.Home:
                    BeforeClose();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }

        }

        public override void OnBackPressed()
        {
            BeforeClose();
        }

        public void OnDismiss(IDialogInterface dialog)
        {
            Finish();
        }

        private void BeforeClose()
        {
            if (listener.state != State.Joined)
            {
                listener.CloseConnection();
                Finish();
                return;
            }
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetMessage("Are you sure you want to close the connection?");
            alert.SetPositiveButton("Yes", (senderAlert, args) => {
                listener.CloseConnection();
                Finish();
            });
            alert.Show();
        }

        private void StartListening()
        {
            try
            {
                listener.Listen();
            } catch (RoomListenerException e)
            {
                RunOnUiThread(() => {
                    Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                    alert.SetTitle("Error while connecting to room");
                    alert.SetMessage(e.Message);
                    alert.SetOnDismissListener(this);
                    alert.Show();
                });
            } catch (Exception e)
            {
                RunOnUiThread(() => {
                    Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                    alert.SetTitle("Unexpected error");
                    alert.SetMessage(e.Message);
                    alert.SetOnDismissListener(this);
                    alert.Show();
                });
            }
        }

        private void UpdateInfoRoom(object sender, Room room)
        {
            if (this.room == null)
            {
                RunOnUiThread(() => {
                    ActionBar.Title = room.name;
                    ActionBar.Subtitle = room.preferredGameName;
                }); 

                Random r = new Random();
                while (unusedColors.Count < room.maxPlayers)
                {
                    Color randColor = new Color(r.Next(255), r.Next(255), r.Next(255));
                    float[] hsv = new float[3];
                    Color.ColorToHSV(randColor, hsv);
                    hsv[2] *= 0.7f;
                    randColor = Color.HSVToColor(hsv);
                    unusedColors.Add(randColor);
                }

                foreach (Player p in room.players)
                {
                    SetUpColor(p);
                }
            }
            else
            {
                foreach (Player p in room.players)
                {
                    if (!this.room.players.Contains(p)) SetUpColor(p);
                }
                foreach (Player p in this.room.players)
                {
                    if (!room.players.Contains(p)) RemoveColor(p);
                }
            }
            this.room = room;
        }

        private void Connected(object sender, EventArgs e)
        {
            RunOnUiThread(() => {
                messageContainer.RemoveView(FindViewById(Resource.Id.connectingLayout));
            });
        }

        private void AddMessage(object sender, ChatMessage msg)
        {
            RunOnUiThread(() => {
                messageContainer.AddView(new ChatMessageView(this, msg, this.colors[msg.Nickname]));
                newMessage.Start();
            });
            
        }

        private void AddStatusMessage(object sender, StatusMessage msg)
        {
            RunOnUiThread(() => {
                messageContainer.AddView(new ChatAlertView(this, msg));
            });
        }

        private void SendMessage(object sender, EventArgs e)
        {
            EditText t = (EditText)FindViewById(Resource.Id.message);
            if (t.Text == "" || room == null) return;
            ChatMessage chmsg = new ChatMessage
            {
                Username = null,
                Message = t.Text,
                TimeStamp = DateTime.Now
            };
            RunOnUiThread(() => messageContainer.AddView(new ChatMessageView(this, chmsg, Color.White)));
            listener.SendChatMessage(t.Text);
            t.Text = "";
        }

        private void OnScroll(object sender, EventArgs e)
        {
            ScrollView scrollView = (ScrollView)sender;
            View view = (View)scrollView.GetChildAt(scrollView.ChildCount - 1);
            int diff = (view.Bottom - (scrollView.Height + scrollView.ScrollY));

            this.bottomReached = (diff == 0);
        }

        private Color SetUpColor(Player p)
        {
            Random r = new Random();
            Color[] colors = unusedColors.ToArray();
            Color choosedColor = colors[r.Next(colors.Length)];
            this.colors[p.nickname] = choosedColor;
            unusedColors.Remove(choosedColor);
            return choosedColor;
        }

        private void RemoveColor(Player p)
        {
            Color c = this.colors[p.nickname];
            this.colors.Remove(p.nickname);
            unusedColors.Add(c);
        }

    }
}