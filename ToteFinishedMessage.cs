namespace gamemaster
{
    public class ToteFinishedMessage
    {
        public ToteFinishedMessage(string toteId, string optionId,
            string userId)
        {
            ToteId = toteId;
            OptionId = optionId;
            UserId = userId;
        }

        public string ToteId { get; }
        public string OptionId { get; }
        public string UserId { get; }
    }
}