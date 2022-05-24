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
                Discard = "topic1,type1,reason1;topic2,type2,reason2;topic3,type3,reason3",
                Park = "topic1,type1,reason1;topic2,type2,reason2;topic3,type3,reason3",
                Retry = "topic1,type1,reason1;topic2,type2,reason2;topic3,type3,reason3"
            };

            Assert.AreEqual(3, rulesOptions.DiscardRules.Count);
            Assert.AreEqual(3, rulesOptions.DiscardRules.Count);
            Assert.AreEqual(3, rulesOptions.DiscardRules.Count);

            Assert.AreEqual("topic1", rulesOptions.DiscardRules[0].OriginalTopic);
            Assert.AreEqual("type1", rulesOptions.DiscardRules[0].MessageType);
            Assert.AreEqual("reason1", rulesOptions.DiscardRules[0].DeathReason);
        }
    }
}
