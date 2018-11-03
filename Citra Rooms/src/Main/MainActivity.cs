using System;
using System.Collections.Generic;
using System.Threading;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;
using Android.Preferences;
using Android.Views;

using CitraRooms.CustomViews;
using CitraRooms.Settings;

namespace CitraRooms.Main
{
    [Activity(Label = "Citra Rooms", Theme = "@android:style/Theme.Material", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private LobbyUpdater updater;
        private Dictionary<String, VRoom> rooms;
        private ISharedPreferences preferences;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Utility.ImportGames(Assets.Open("games.json"));

            updater = new LobbyUpdater(5000);
            updater.OnRoomUpdate += ElaborateLobbyData;
            new Thread(() => {
                updater.UpdateLobby(null, null);
                RunOnUiThread(() => ((RelativeLayout)FindViewById(Resource.Id.mainLayout)).RemoveView((LinearLayout)FindViewById(Resource.Id.loadingPage)));
            }).Start();

            ActionBar.Title = "Citra Rooms";

            rooms = new Dictionary<string, VRoom>();

            preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {

            switch (item.ItemId)
            {
                case Resource.Id.openSetting:
                    Intent intent = new Intent(this.ApplicationContext, typeof(SettingsActivity));
                    StartActivity(intent);
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }

        }

        protected override void OnResume()
        {
            base.OnResume();
            updater.Start();
        }

        protected override void OnPause()
        {
            base.OnStop();
            updater.Stop();
        }

        private void ElaborateLobbyData(Object sender, List<Room> data)
        {
            var deleteRooms = new Dictionary<String, VRoom>(rooms);
            var addRooms = new Dictionary<int, VRoom>();
            var replaceRooms = new Dictionary<VRoom, Tuple<int, VRoom>>();
            var layout = ((LinearLayout)FindViewById(Resource.Id.listRooms));
            for (int i = 0; i < data.Count; i++)
            {
                Room room = data[i];
                string key = room.address + ":" + room.port.ToString();
                
                if (!rooms.ContainsKey(key))
                {
                    VRoom view = new VRoom(this, room);
                    rooms.Add(key, view);
                    addRooms[i] = view;
                }
                else
                {
                    deleteRooms.Remove(key);
                    if (!rooms[key].Equals(room))
                    {
                        VRoom oldView = rooms[key];
                        VRoom view = new VRoom(this, room);
                        rooms[key] = view;
                        replaceRooms[oldView] = new Tuple<int, VRoom>(i, view);
                    } else
                    {
                        rooms[key].room = room;
                    }
                }
            }
            
            RunOnUiThread(() => {
                foreach (VRoom view in deleteRooms.Values)
                {
                    layout.RemoveView(view);
                    rooms.Remove(view.room.address + ":" + view.room.port.ToString());
                }
                foreach (var entry in addRooms)
                {
                    layout.AddView(entry.Value, entry.Key);
                }
                foreach (var entry in replaceRooms)
                {
                    layout.RemoveView(entry.Key);
                    layout.AddView(entry.Value.Item2, entry.Value.Item1);
                }
            });

        }
        
        
    }
}