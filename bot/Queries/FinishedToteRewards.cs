using gamemaster.Models;

namespace gamemaster.Queries
{
    public class FinishedToteRewards
    {
        public AccountWithAmount[] ProportionalReward { get; }
        public string WinningOptionName { get;  }
        public ToteBet[] WinningBets { get;  }
        public decimal OwnerPercent { get; }

        public FinishedToteRewards(AccountWithAmount[] proportionalReward, ToteOption winningOption,
            decimal ownerPercent)
        {
            ProportionalReward = proportionalReward;
            OwnerPercent = ownerPercent;
            WinningBets = winningOption.Bets;
            WinningOptionName = winningOption.Name;
        }

    }
}