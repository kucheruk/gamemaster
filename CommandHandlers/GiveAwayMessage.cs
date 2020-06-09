namespace gamemaster.CommandHandlers
{
    public class GiveAwayMessage
    {
        public GiveAwayMessage(string fromUser, string currency,
            string responseUrl, in decimal amount,
            string[] users, MessageContext channel,
            string comment)
        {
            FromUser = fromUser;
            Currency = currency;
            ResponseUrl = responseUrl;
            Amount = amount;
            Users = users;
            Channel = channel;
            Comment = comment;
        }

        public string FromUser { get; }
        public string Currency { get; }
        public string ResponseUrl { get; }
        public decimal Amount { get; }
        public string[] Users { get; }
        public MessageContext Channel { get; }
        public string Comment { get; }
    }
}