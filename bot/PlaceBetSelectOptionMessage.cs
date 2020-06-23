namespace gamemaster
{
    public class PlaceBetSelectOptionMessage 
    {
        public string UserId { get; }
        public string ToteId { get; }
        public string OptionId { get; }

        public PlaceBetSelectOptionMessage(string userId, string toteId,
            string optionId)
        {
            UserId = userId;
            ToteId = toteId;
            OptionId = optionId;
        }
    }
}