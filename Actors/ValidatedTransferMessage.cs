namespace gamemaster.Actors
{
    public class ValidatedTransferMessage
    {
        
        public ValidatedTransferMessage(string fromAccount, string toAccount,
            decimal amount, string currency,
            string comment, bool toServiceAccount = false,
            string fromUserCaption = null)
        {
            FromAccount = fromAccount;
            ToToAccount = toAccount;
            Amount = amount;
            Currency = currency;
            Comment = comment;
            ToServiceAccount = toServiceAccount;
            FromUserCaption = fromUserCaption;
        }

        public string FromAccount { get; }
        public string ToToAccount { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public string Comment { get; }
        public bool ToServiceAccount { get; }
        public string FromUserCaption { get;  }
    }
}