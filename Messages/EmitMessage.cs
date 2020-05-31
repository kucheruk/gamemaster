namespace gamemaster.Messages
{
    public class EmitMessage
    {
        public EmitMessage(string toUser, string currency,
            in int amount, string adminId, string responseUrl)
        {
            ToUser = toUser;
            Currency = currency;
            Amount = amount;
            AdminId = adminId;
            ResponseUrl = responseUrl;
        }

        public string ToUser { get; }
        public string Currency { get; }
        public int Amount { get; }
        public string AdminId { get; }
        public string ResponseUrl { get; }
    }
}