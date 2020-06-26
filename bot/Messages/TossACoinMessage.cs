namespace gamemaster.Messages
{
    public class TossACoinMessage
    {
        public TossACoinMessage(string fromUser, string currency,
            string responseUrl, decimal amount,
            string user, string comment,
            bool tossAll)
        {
            FromUser = fromUser;
            Currency = currency;
            ResponseUrl = responseUrl;
            Amount = amount;
            ToUser = user;
            Comment = comment;
            TossAll = tossAll;
        }

        public string FromUser { get; }
        public string Currency { get; }

        public string ResponseUrl { get; }
        public decimal Amount { get; }
        public string ToUser { get; }
        public string Comment { get; }
        public bool TossAll { get; }
    }
}