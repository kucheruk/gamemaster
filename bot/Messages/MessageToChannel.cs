namespace gamemaster.Messages
{
    public class MessageToChannel
    {
        public MessageToChannel(string channelId, string message)
        {
            ChannelId = channelId;
            Message = message;
        }

        public bool Ephemeral { get; }
        public string ChannelId { get; }
        public string Message { get; }
        public string UserId { get; set; }
    }
}