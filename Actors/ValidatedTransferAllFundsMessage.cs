namespace gamemaster.Actors
{
    public class ValidatedTransferAllFundsMessage
    {
        public string FromAccount { get; }
        public string ToAccount { get; }
        public string Currency { get; }
        public string OpDesc { get; }
        public string FromCaption { get; }

        public ValidatedTransferAllFundsMessage(string fromAccount, string toAccount,
            string currency, string opDesc, string fromCaption)
        {
            FromAccount = fromAccount;
            ToAccount = toAccount;
            Currency = currency;
            OpDesc = opDesc;
            FromCaption = fromCaption;
            throw new System.NotImplementedException();
        }
    }
}