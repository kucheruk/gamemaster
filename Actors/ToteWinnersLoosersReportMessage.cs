using gamemaster.Models;

namespace gamemaster.Actors
{
    public class ToteWinnersLoosersReportMessage
    {
        public string Option { get; }
        public string ToteId { get; }
        public ToteBet[] WinningBets { get; }
        public AccountWithAmount[] Rewards { get; }
        public decimal OwnerPercent { get; }
        public string ToteOwner { get; }

        public ToteWinnersLoosersReportMessage(string option, string toteId,
            ToteBet[] winningBets, AccountWithAmount[] rewards,
            in decimal ownerPercent, string toteOwner)
        {
            Option = option;
            ToteId = toteId;
            WinningBets = winningBets;
            Rewards = rewards;
            OwnerPercent = ownerPercent;
            ToteOwner = toteOwner;
        }
        
        
        
    }
}