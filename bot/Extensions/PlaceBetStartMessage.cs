namespace gamemaster.Extensions
{
    public  class PlaceBetStartMessage
    {
        public PlaceBetStartMessage(string userId, string toteId,
            string responseUrl, string triggerId)
        {
            UserId = userId;
            ToteId = toteId;
            ResponseUrl = responseUrl;
            TriggerId = triggerId;
        }

        public string UserId { get; }
        public string ToteId { get; }
        public string ResponseUrl { get; }
        public string TriggerId { get; set; }
    }
}