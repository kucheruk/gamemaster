using gamemaster.Models;

namespace gamemaster.Messages
{
    public class ToteWinnersLoosersReportMessage
    {
        public ToteWinnersLoosersReportMessage(FinishedToteRewards rewards, Tote tote)
        {
            Option = rewards.WinningOptionName;
            WinningBets = rewards.WinningBets;
            Rewards = rewards.ProportionalReward;
            OwnerPercent = rewards.OwnerPercent;
            ToteId = tote.Id;
            ToteOwner = tote.Owner;
        }

        public string Option { get; }
        public string ToteId { get; }
        public ToteBet[] WinningBets { get; }
        public AccountWithAmount[] Rewards { get; }
        public decimal OwnerPercent { get; }
        public string ToteOwner { get; }
    }
}