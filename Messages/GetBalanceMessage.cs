namespace gamemaster.Messages
{
    public class GetBalanceMessage
    {
        public GetBalanceMessage(string userId, bool admin, MessageContext context, string responseUrl)
        {
            UserId = userId;
            Admin = admin;
            Context = context;
            ResponseUrl = responseUrl;
        }

        public MessageContext Context { get; }
        public bool Admin { get; }
        public string UserId { get;  }
        public string ResponseUrl { get; }
    }
}