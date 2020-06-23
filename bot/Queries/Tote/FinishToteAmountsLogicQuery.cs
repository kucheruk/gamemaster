using System.Linq;
using gamemaster.Extensions;
using gamemaster.Models;

namespace gamemaster.Queries.Tote
{
    public class FinishToteAmountsLogicQuery
    {
        public FinishedToteRewards CalcRewards(Models.Tote tote, string winningOptionId)
        {
            var bets = tote.Options.SelectMany(a => a.Bets);
            var totalSum = bets.Sum(a => a.Amount);

            var ownerPercent = totalSum / 100;
            var winningFund = totalSum - ownerPercent;

            ToteOption winningOption = tote.Options.FirstOrDefault(a => a.Id == winningOptionId);
            var winningBets = winningOption.Bets;
            var winningBetsSum = winningBets.Sum(a => a.Amount);

            var ama = winningBets.Select(a => new AccountWithAmount(new Account(a.User, tote.Currency), a.Amount));
            var agg = ama.AggregateOperations();
            var proportions = agg.Select(a => new AccountWithAmount(a.Account, a.Amount / winningBetsSum));
            AccountWithAmount[] proportionalReward = proportions.Select(a =>
                new AccountWithAmount(a.Account, decimal.Round(a.Amount * winningFund, 2))).ToArray();
            return new FinishedToteRewards(proportionalReward, winningOption, ownerPercent);
        }
    }
}