namespace gamemaster.Messages
{
    public class TossACoinMessage
    {
        public TossACoinMessage(string fromUser, string currency,
            string responseUrl, int amount,
            string user)
        {
            FromUser = fromUser;
            Currency = currency;
            ResponseUrl = responseUrl;
            Amount = amount;
            ToUser = user;
        }

        public string FromUser { get; }
        public string Currency { get; }

        public string ResponseUrl { get; }
        public int Amount { get; }
        public string ToUser { get; }
    }
}