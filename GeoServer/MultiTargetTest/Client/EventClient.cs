using System;

namespace GeoStreamer
{
    public class MessageArgs : EventArgs
    {
        private readonly string message;

        public MessageArgs(string message)
        {
            this.message = message;
        }

        public string Message => message;
    }
    public class EventClient : Client<EventClient>
    {
        public event EventHandler<MessageArgs> Message;

        protected override void SendLog(string message)
        {
            Message?.Invoke(this, new MessageArgs(message));
        }
    }
}