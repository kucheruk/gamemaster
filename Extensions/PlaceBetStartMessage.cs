namespace gamemaster.Extensions
{
    public  class PlaceBetStartMessage
    {
        public PlaceBetStartMessage(string userId, string toteId)
        {
            UserId = userId;
            ToteId = toteId;
        }

        public string UserId { get; }
        public string ToteId { get; }
    }
}