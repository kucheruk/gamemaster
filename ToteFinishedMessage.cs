namespace gamemaster
{
    public class ToteFinishedMessage
    {
        public string ToteId { get; }
        public int WinningNumber { get; }

        public ToteFinishedMessage(string toteId, in int winningNumber)
        {
            ToteId = toteId;
            WinningNumber = winningNumber;
        }
    }
}