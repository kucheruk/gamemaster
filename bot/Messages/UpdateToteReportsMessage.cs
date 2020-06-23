namespace gamemaster.Actors
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