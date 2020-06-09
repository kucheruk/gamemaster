namespace gamemaster.Actors
{
    public class ValidatedTransferMessage
    {
        public ValidatedTransferMessage(string fromUser, string user,
            decimal amount, string currency,
            string comment)
        {
            FromUser = fromUser;
            ToUser = user;
            Amount = amount;
            Currency = currency;
            Comment = comment;
        }

        public string FromUser { get; }
        public string ToUser { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public string Comment { get; }
    }
}