namespace gamemaster
{
    public class TotePlaceBetMessage
    {
        public TotePlaceBetMessage(string user, string toteId,
            in string optionId, in decimal amount)
        {
            User = user;
            ToteId = toteId;
            OptionId = optionId;
            Amount = amount;
        }

        public string User { get; }
        public string ToteId { get; }
        public string OptionId { get; }
        public decimal Amount { get; }
    }
}