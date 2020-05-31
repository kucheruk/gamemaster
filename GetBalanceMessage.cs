namespace gamemaster
{
    public class GetBalanceMessage
    {
        public GetBalanceMessage(string userId, string responseUrl)
        {
            UserId = userId;
            ResponseUrl = responseUrl;
        }

        public string UserId { get; set; }
        public string ResponseUrl { get; set; }
    }
}