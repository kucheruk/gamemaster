using gamemaster.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests
{
    [TestClass]
    public class TestTossCoinCommandParser
    {
        [TestMethod]
        public void TestParamsFromText()
        {
            var p = TossRequestParams.FromText("<@TEST1|user> :coin: 1 test test test");
            Assert.IsNotNull(p);
            Assert.AreEqual("TEST1", p.UserId);
            Assert.AreEqual(":coin:", p.Currency);
            Assert.AreEqual(1m, p.Amount);
            Assert.AreEqual("test test test", p.Comment);
        }

        [TestMethod]
        public void TestDecimal()
        {
            var p = TossRequestParams.FromText(":smile_aa: 100 <@A1B2C3|user> ");
            Assert.IsNotNull(p);
            Assert.AreEqual("A1B2C3", p.UserId);
            Assert.AreEqual(":smile_aa:", p.Currency);
            Assert.AreEqual(100m, p.Amount);
            p = TossRequestParams.FromText("<@A1B2C3|user> :coin: 12345.67");
            Assert.AreEqual(12345.67m, p.Amount);
            p = TossRequestParams.FromText("<@A1B2C3|user> :coin: 12345.6783871362");
            Assert.AreEqual(12345.68m, p.Amount);
        } 
        
        [TestMethod]
        public void TestNegative()
        {
            var p = TossRequestParams.FromText("aaaa bbbb test");
            Assert.IsNotNull(p);
            Assert.IsNull( p.UserId);
            Assert.IsNull(null, p.Currency);
            Assert.AreEqual(0, p.Amount);
        }
    }
}
