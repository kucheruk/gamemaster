namespace gamemaster
{
    public readonly struct AccountWithAmount
    {
        public AccountWithAmount(Account account, decimal amount)
        {
            Account = account;
            Amount = amount;
        }

        public Account Account { get; }
        public decimal Amount { get; }

        public override string ToString()
        {
            return $"=={Account.UserId} [{Amount}]{Account.Currency}";
        }
    }
}