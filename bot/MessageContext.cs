using gamemaster.Messages;

namespace gamemaster
{
    public struct MessageContext
    {
        public MessageContext(ChannelType type, string channelId)
        {
            Type = type;
            ChannelId = channelId;
        }

        public ChannelType Type { get; set; }
        public string ChannelId { get; set; }
    }
}