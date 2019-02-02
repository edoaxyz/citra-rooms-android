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
            if (msg.Nickname == null)
            {
                View v = Inflate(Context, Resource.Layout.layout_sent_message, this);
                this.SetGravity(GravityFlags.Right);
            }
            else
            {
                Inflate(Context, Resource.Layout.layout_message, this);

                label = (TextView)FindViewById(Resource.Id.usernameMessage);
                label.Text = msg.Nickname;
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
        public StatusMessage message;

        public ChatAlertView(Context context, StatusMessage message) : base(context)
        {
            this.message = message;

            SetUI();
        }


        private void SetUI()
        {
            Inflate(Context, Resource.Layout.layout_chat_alert, this);
            this.SetGravity(GravityFlags.Center);

            String text = message.Nickname;
            switch (message.Type)
            {
                case StatusMessageTypes.IdMemberJoin:
                    text += " joined room";
                    break;
                case StatusMessageTypes.IdMemberLeave:
                    text += " leaved room";
                    break;
                case StatusMessageTypes.IdMemberKicked:
                    text += " kicked from room";
                    break;
                case StatusMessageTypes.IdMemberBanned:
                    text += " banned from room";
                    break;
                case StatusMessageTypes.IdAddressUnbanned:
                    text += " unbanned from room";
                    break;
            }

            TextView label = (TextView)FindViewById(Resource.Id.messageAlert);
            label.Text = text;

            label = (TextView)FindViewById(Resource.Id.timeAlert);
            label.Text = message.TimeStamp.ToShortTimeString();
        }
    }
}