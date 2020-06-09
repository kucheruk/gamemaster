using SlackAPI;

namespace gamemaster.Actors
{
    internal class BlocksMessage
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