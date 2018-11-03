using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Preferences;

using CitraRooms.Main;
using CitraRooms.Settings;
using CitraRooms.Rooms;

namespace CitraRooms.CustomViews
{
    class VRoom : LinearLayout
    {
        public Room room;

        public VRoom(Context context, Room room) : base(context)
        {
            this.room = room;
            SetUI();
        }

        private void SetUI()
        {
            Inflate(Context, Resource.Layout.layout_room, this);
            TextView label = (TextView)FindViewById(Resource.Id.roomLabel);
            label.Text = room.name;
            label = (TextView)FindViewById(Resource.Id.gameLabel);
            label.Text = room.preferredGameName;
            label = (TextView)FindViewById(Resource.Id.ownerLabel);
            label.Text = room.owner;
            label = (TextView)FindViewById(Resource.Id.playersCounter);
            if (room.players != null) label.Text = room.players.Count.ToString() + " / " + room.maxPlayers.ToString();
            else label.Text = "0 / " + room.maxPlayers.ToString();

            Bitmap final;
            String imageFile = Utility.MatchAvailableGame(room.preferredGameId, Context);

            Bitmap image;
            if (imageFile != null) image = BitmapFactory.DecodeStream(Context.Assets.Open("game_" + imageFile + ".png"));
            else image = BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.ic_citra);

            if (room.hasPassword)
            {
                Bitmap overlay = BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.ic_lock);
                final = Utility.RoundImage(image, overlay);
            }
            else
            {
                final = Utility.RoundImage(image, null);
            }

            ImageView icon = (ImageView)FindViewById(Resource.Id.gameImage);
            icon.SetImageBitmap(final);

            ImageButton button = (ImageButton)FindViewById(Resource.Id.infoPlayers);
            button.Click += CallDialog;

            button = (ImageButton)FindViewById(Resource.Id.enterRoom);
            button.Click += OpenRoom;
        }

        public void CallDialog(Object sender, EventArgs e)
        {
            new InfoRoomDialog(Context, room).Show();
        }

        public void OpenRoom(Object sender, EventArgs e)
        {
            ISharedPreferences preferences = PreferenceManager.GetDefaultSharedPreferences(Context);
            String username = preferences.GetString("username", "");
            if (username.Length < 4 || username.Length > 20)
            {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(Context);
                alert.SetTitle("Username not valid");
                alert.SetMessage("Username must be around 4 and 20 characters.");
                alert.SetPositiveButton("Change it", (senderAlert, args) => {
                    Intent intent = new Intent(Context, typeof(SettingsActivity));
                    Context.StartActivity(intent);
                });
                alert.Show();
                return;
            }
            
            Intent i = new Intent(Context, typeof(RoomActivity));
            i.PutExtra("address", room.address);
            i.PutExtra("port", room.port);
            i.PutExtra("username", username);

            if (room.hasPassword)
            {
                EditText input = new EditText(Context);
                input.Hint = "Password";
                input.Text = preferences.GetString("default_password", "");
                input.InputType = Android.Text.InputTypes.TextVariationPassword | Android.Text.InputTypes.ClassText;
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(Context);
                alert.SetTitle("Insert password");
                alert.SetView(input);
                alert.SetPositiveButton("Join Room", (senderAlert, args) => {
                    i.PutExtra("password", input.Text);
                    Context.StartActivity(i);
                });
                alert.Show();
            } else
            {
                i.PutExtra("password", "");
                Context.StartActivity(i);
            }
        }
    }

    class VPlayer : LinearLayout
    {
        public Player player;

        public VPlayer(Context context, Player player) : base(context)
        {
            this.player = player;
            SetUI();
        }

        private void SetUI()
        {
            Inflate(Context, Resource.Layout.layout_player, this);
            TextView label = (TextView)FindViewById(Resource.Id.playerName);
            label.Text = player.name;
            label = (TextView)FindViewById(Resource.Id.playerGame);
            label.Text = player.gameName;
            
            String imageFile = Utility.MatchAvailableGame(player.gameId, Context);

            Bitmap image;
            if (imageFile != null) image = BitmapFactory.DecodeStream(Context.Assets.Open("game_" + imageFile + ".png"));
            else image = BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.ic_citra);
            image = Bitmap.CreateScaledBitmap(image, 48, 48, false);

            ImageView icon = (ImageView)FindViewById(Resource.Id.player_gameImage);
            icon.SetImageBitmap(Utility.RoundImage(image, null));

        }
    }

    class InfoRoomDialog : Dialog
    {
        public Room room;

        public InfoRoomDialog(Context context,Room room) : base(context)
        {
            this.room = room;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature((int)WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.players_dialog);

            SetUI();
        }

        private void SetUI()
        {
            LinearLayout listPl = (LinearLayout)FindViewById(Resource.Id.playersList);

            TextView label = (TextView)FindViewById(Resource.Id.dialog_roomLabel);
            label.Text = room.name;
            label = (TextView)FindViewById(Resource.Id.dialog_gameLabel);
            label.Text = room.preferredGameName;
            label = (TextView)FindViewById(Resource.Id.dialog_ownerLabel);
            label.Text = room.owner;
            label = (TextView)FindViewById(Resource.Id.dialog_playerCounter);
            label.Text = room.owner;
            if (room.players != null)
            {
                label.Text = room.players.Count.ToString() + " / " + room.maxPlayers.ToString();
                foreach (Player p in room.players)
                {
                    listPl.AddView(new VPlayer(Context, p));
                }
            }
            else label.Text = "0 / " + room.maxPlayers.ToString();

            Bitmap final;
            String imageFile = Utility.MatchAvailableGame(room.preferredGameId, Context);

            Bitmap image;
            if (imageFile != null) image = BitmapFactory.DecodeStream(Context.Assets.Open("game_" + imageFile + ".png"));
            else image = BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.ic_citra);

            if (room.hasPassword)
            {
                Bitmap overlay = BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.ic_lock);
                final = Utility.RoundImage(image, overlay);
            }
            else
            {
                final = Utility.RoundImage(image, null);
            }

            ImageView icon = (ImageView)FindViewById(Resource.Id.dialog_gameImage);
            icon.SetImageBitmap(final);

        }
    }

    
}