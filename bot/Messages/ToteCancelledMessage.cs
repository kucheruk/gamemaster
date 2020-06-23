namespace gamemaster.Messages
{
    public class ToteCancelledMessage
    {
        public string ToteId { get; }

        public ToteCancelledMessage(string toteId)
        {
            ToteId = toteId;
        }
    }
}