using System.Diagnostics;

namespace gamemaster.Messages
{
    public class MessageToChannel
    {
        public MessageToChannel(string channelId, string message)
        {
            ChannelId = channelId;
            if (string.IsNullOrEmpty(channelId))
            {
            }

            Message = message;
        }

        public string ChannelId { get; set; }
        public string Message { get; set; }
    }
}