using System;

using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

using CitraRooms.Rooms;

namespace CitraRooms.CustomViews.Chat
{
    class ChatMessageView : LinearLayout
    {
        public ChatMessage msg;
        public Color usernameColor;

        public ChatMessageView(Context context, ChatMessage msg, Color usernameColor) : base(context)
        {
            this.msg = msg;
            this.usernameColor = usernameColor;

            SetUI();
        }

        private void SetUI()
        {
            TextView label;
            if (msg.Username == null)
            {
                View v = Inflate(Context, Resource.Layout.layout_sent_message, this);
                this.SetGravity(GravityFlags.Right);
            }
            else
            {
                Inflate(Context, Resource.Layout.layout_message, this);

                label = (TextView)FindViewById(Resource.Id.usernameMessage);
                label.Text = msg.Username;
                label.SetTextColor(this.usernameColor);
            }

            label = (TextView)FindViewById(Resource.Id.textMessage);
            label.Text = msg.Message;

            label = (TextView)FindViewById(Resource.Id.timeMessage);
            label.Text = msg.TimeStamp.ToShortTimeString();

        }
    }

    class ChatAlertView : LinearLayout
    {
        public String message;
        public DateTime timeStamp;

        public ChatAlertView(Context context, String message) : base(context)
        {
            this.message = message;
            this.timeStamp = DateTime.Now;

            SetUI();
        }


        private void SetUI()
        {
            Inflate(Context, Resource.Layout.layout_chat_alert, this);
            this.SetGravity(GravityFlags.Center);

            TextView label = (TextView)FindViewById(Resource.Id.messageAlert);
            label.Text = message;

            label = (TextView)FindViewById(Resource.Id.timeAlert);
            label.Text = timeStamp.ToShortTimeString();
        }
    }
}