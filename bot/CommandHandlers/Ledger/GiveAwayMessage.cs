namespace gamemaster.CommandHandlers.Ledger
{
    public class GiveAwayMessage
    {
        public GiveAwayMessage(string fromUser, string currency,
            string responseUrl, in decimal amount,
            string[] users, MessageContext channel,
            string comment, bool tossAll)
        {
            FromUser = fromUser;
            Currency = currency;
            ResponseUrl = responseUrl;
            Amount = amount;
            Users = users;
            Channel = channel;
            Comment = comment;
            TossAll = tossAll;
        }

        public string FromUser { get; }
        public string Currency { get; }
        public string ResponseUrl { get; }
        public decimal Amount { get; }
        public string[] Users { get; }
        public MessageContext Channel { get; }
        public string Comment { get; }
        public bool TossAll { get; }
    }
}