namespace gamemaster.Queries.Ledger
{
    public class GetAccountBalanceRequestMessage
    {
        public string UserId { get; set; }
        public string Currency { get; set; }
    }
}