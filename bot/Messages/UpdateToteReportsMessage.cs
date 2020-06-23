namespace gamemaster.Messages
{
    internal class UpdateToteReportsMessage
    {
        public string ToteId { get; }

        public UpdateToteReportsMessage(string toteId)
        {
            ToteId = toteId;
        }
    }
}