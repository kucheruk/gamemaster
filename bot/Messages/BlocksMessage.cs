using SlackAPI;

namespace gamemaster.Messages
{
    public class BlocksMessage
    {
        public BlocksMessage(IBlock[] blocks, string channelId)
        {
            Blocks = blocks;
            ChannelId = channelId;
        }
        public IBlock[] Blocks { get; }
        public string ChannelId { get; }
    }
}