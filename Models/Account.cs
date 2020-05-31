namespace gamemaster
{
    public readonly struct Account
    {
        public Account(string userId, string currency)
        {
            UserId = userId;
            Currency = currency;
        }

        public string UserId { get; }
        public string Currency { get; }
    }
}