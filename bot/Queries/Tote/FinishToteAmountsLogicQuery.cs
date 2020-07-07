using System;
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

            var ownerPercent = totalSum / 20; //5% more fun than 1%
            var winningFund = totalSum - ownerPercent;

            ToteOption winningOption = tote.Options.FirstOrDefault(a => a.Id == winningOptionId);
            var winningBets = winningOption.Bets;
            var winningBetsSum = winningBets.Sum(a => a.Amount);

            var ama = winningBets.Select(a => new AccountWithAmount(new Account(a.User, tote.Currency), a.Amount));
            var agg = ama.AggregateOperations();
            
            // var proportions = agg.Select(a => new AccountWithAmount(a.Account, a.Amount / winningBetsSum));
            
            AccountWithAmount[] proportionalReward = agg.Select(a =>
                new AccountWithAmount(a.Account, ((a.Amount * winningFund) / winningBetsSum).Trim())).ToArray();
            
            ownerPercent = AdjustRemainderToOwner(proportionalReward, winningFund, ownerPercent);

            return new FinishedToteRewards(proportionalReward, winningOption, ownerPercent);
        }

        private static decimal AdjustRemainderToOwner(AccountWithAmount[] proportionalReward, decimal winningFund,
            decimal ownerPercent)
        {
            var totalReward = proportionalReward.Sum(a => a.Amount); // calc remainder from rounding decimals 
            var remainder = winningFund - totalReward;
            return ownerPercent + remainder;
        }
    }
}