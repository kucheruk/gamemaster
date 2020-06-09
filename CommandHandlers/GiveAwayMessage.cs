namespace gamemaster.CommandHandlers
{
    public class GiveAwayMessage
    {
        public string FromUser { get; }
        public string Currency { get; }
        public string ResponseUrl { get; }
        public int Amount { get; }
        public string[] Users { get; }

        public GiveAwayMessage(string fromUser, string currency,
            string responseUrl, in int amount,
            string[] users)
        {
            FromUser = fromUser;
            Currency = currency;
            ResponseUrl = responseUrl;
            Amount = amount;
            Users = users;
        }
    }
}