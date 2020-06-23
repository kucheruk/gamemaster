namespace gamemaster.Messages
{
    public class EphemeralMessageToChannel
    {
        public EphemeralMessageToChannel(string channelId,string userId, string message)
        {
            ChannelId = channelId;
            UserId = userId;
            Message = message;
        }

        public string ChannelId { get; }
        public string Message { get; }
        public string UserId { get; }
    }
}