using System.ComponentModel.DataAnnotations;
using gamemaster.Extensions;
using gamemaster.Models;
using gamemaster.Queries.Tote;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests
{
    [TestClass]
    public class TestFinishToteRewards
    {
        [TestMethod]
        public void TestSimpleRewards()
        {
            var q = new FinishToteAmountsLogicQuery();
            var t = new Tote
            {
                Owner = "owner",
                Options = new[]
                {
                    new ToteOption {Id = "o1", Bets = new[] {new ToteBet {Amount = 100, User = "1"}, new ToteBet {Amount = 200, User = "2"}}},
                    new ToteOption {Id = "o2", Bets = new[] {new ToteBet {Amount = 300, User = "3"}, new ToteBet {Amount = 300, User = "4"}}}
                }
            };
            var r = q.CalcRewards(t, "o1");
            decimal total = 900;
            decimal percent = total / 20m;
            decimal onethird = ((900 - percent) / 3).Trim();
            decimal twothird = (2 * ((900 - percent) / 3)).Trim();
            decimal remainder = total - percent - onethird - twothird;
            Assert.AreEqual(r.OwnerPercent, remainder + percent);
            Assert.AreEqual(onethird, r.ProportionalReward[0].Amount);
            Assert.AreEqual(twothird, r.ProportionalReward[1].Amount);
            Assert.AreEqual("1", r.ProportionalReward[0].Account.UserId);
            Assert.AreEqual("2", r.ProportionalReward[1].Account.UserId);
        }
    }
}