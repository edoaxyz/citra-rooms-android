using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace CitraRooms.Settings
{
    [Activity(Label = "Settings", Theme = "@android:style/Theme.Material", MainLauncher = false)]
    public class SettingsActivity : Activity
    {
        private ISharedPreferences preferences;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_settings);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);
            SetUI();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId != Android.Resource.Id.Home) return base.OnOptionsItemSelected(item);
            Finish();
            return true;
        }

        private void SetUI()
        {
            EditText input = (EditText)FindViewById(Resource.Id.username);
            input.AfterTextChanged += TextChanged;
            input.Text = preferences.GetString("username", "");
            input = (EditText)FindViewById(Resource.Id.defPassword);
            input.AfterTextChanged += TextChanged;
            input.Text = preferences.GetString("default_password", "");
        }

        private void TextChanged(object sender, EventArgs e)
        {
            ISharedPreferencesEditor editor = preferences.Edit();
            EditText input = (EditText)sender;
            switch (input.Id)
            {
                case Resource.Id.username:
                    editor.PutString("username", input.Text);
                    if (input.Text.Length < 4)
                    {
                        input.SetTextColor(Color.Red);
                    } else
                    {
                        input.SetTextColor(Color.White);
                    }
                    break;
                case Resource.Id.defPassword:
                    editor.PutString("default_password", input.Text);
                    break;
            }
            editor.Apply();
        }
    }
}