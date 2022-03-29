using Microsoft.DeadLetterMonitor.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.DeadLetterMonitor.Tests 
{
    [TestClass]
    public class DeadLetterMonitorRulesTest 
    {

        /// <summary>
        /// Adds the message to a queue and send a reject - send to dead letter.
        /// This method is only for creating real data for debug.
        /// </summary>
        [TestMethod]
        public void CreateRulesOptions()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Discard = "exchange1,type1,reason1;exchange2,type2,reason2;exchange3,type3,reason3",
                Park = "exchange1,type1,reason1;exchange2,type2,reason2;exchange3,type3,reason3",
                Retry = "exchange1,type1,reason1;exchange2,type2,reason2;exchange3,type3,reason3"
            };

            Assert.AreEqual(3, rulesOptions.DiscardRules.Count);
            Assert.AreEqual(3, rulesOptions.DiscardRules.Count);
            Assert.AreEqual(3, rulesOptions.DiscardRules.Count);

            Assert.AreEqual("exchange1", rulesOptions.DiscardRules[0].OriginalExchange);
            Assert.AreEqual("type1", rulesOptions.DiscardRules[0].MessageType);
            Assert.AreEqual("reason1", rulesOptions.DiscardRules[0].DeathReason);
        }
    }
}
