namespace gamemaster
{
    public class TotePlaceBetMessage
    {
        public string User { get; }
        public string ToteId { get; }
        public int Number { get; }
        public decimal Amount { get; }
        public string ResponseUrl { get; }

        public TotePlaceBetMessage(string user, string toteId,
            in int number, in decimal amount,
            string responseUrl)
        {
            User = user;
            ToteId = toteId;
            Number = number;
            Amount = amount;
            ResponseUrl = responseUrl;
        }
    }
}