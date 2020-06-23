namespace gamemaster.Messages
{
    public class TotePlaceBetMessage
    {
        public TotePlaceBetMessage(string user, string toteId,
            in string optionId, in decimal amount,
            string channelId)
        {
            User = user;
            ToteId = toteId;
            OptionId = optionId;
            Amount = amount;
            ChannelId = channelId;
        }

        public string User { get; }
        public string ToteId { get; }
        public string OptionId { get; }
        public decimal Amount { get; }
        public string ChannelId { get; }
    }
}