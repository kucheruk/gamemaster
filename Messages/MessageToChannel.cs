namespace gamemaster.Messages
{
    internal class MessageToChannel
    {
        public MessageToChannel(string channelId, string message)
        {
            ChannelId = channelId;
            Message = message;
        }

        public string ChannelId { get; set; }
        public string Message { get; set; }
    }
}