namespace gamemaster.Actors
{
    public class ValidatedTransferMessage
    {
        public ValidatedTransferMessage(string fromUser, string user,
            decimal amount, string currency)
        {
            FromUser = fromUser;
            ToUser = user;
            Amount = amount;
            Currency = currency;
        }

        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}