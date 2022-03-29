using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.DeadLetterMonitor.Handlers;
using Microsoft.DeadLetterMonitor.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.DeadLetterMonitor.Tests.Handlers {
    [TestClass]
    public class DelayedMessageHandlerTest {
        /// <summary>
        /// Test discard rules.
        /// </summary>
        [TestMethod]
        public void TestHandleMessageInError()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Discard = "exchange1,type1,reason1;*,type2,reason2;exchange3,*,reason3; exchange4,type4,*;exchange5,*,*;*,type6,*;*,*,reason7",
                Park = "exchange8,type8,reason8",
                Retry = "exchange9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { Rules = rulesOptions };

            var mockPublisher = new GenericPublisherMock();

            var handler = new DelayedMessageHandler(mockPublisher, mockAppInsights);

            var body = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

            var message = new Message("1",  DateTime.UtcNow.ToString(), "type1", "exchange1", "key", null, new Dictionary<string, object>(), body);

            Assert.ThrowsException<ArgumentException>(() => handler.HandleMessage(message));
        }

        /// <summary>
        /// Test discard rules.
        /// </summary>
        [TestMethod]
        public void TestHandleMessage()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Discard = "exchange1,type1,reason1;*,type2,reason2;exchange3,*,reason3; exchange4,type4,*;exchange5,*,*;*,type6,*;*,*,reason7",
                Park = "exchange8,type8,reason8",
                Retry = "exchange9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { MaxRetries = 2, Rules = rulesOptions };

            var mockPublisher = new GenericPublisherMock();

            var handler = new DelayedMessageHandler(mockPublisher, mockAppInsights);

            var msg = GetMessageArgs("exchange1", "type1", "reason1");

            handler.HandleMessage(msg);

            // Check if all messages are published to original queue
            Assert.AreEqual(1, mockPublisher.PublishedMessages.Count(m => m == "exchange1"));
        }


        private IMessage GetMessageArgs(string exchangeName, string type, string reason)
        {
            var body = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

            var message = new Message("1", DateTime.UtcNow.ToString(), type, "exchange1", "key", null, new Dictionary<string, object>(), body);

            message.Headers.Add("x-first-death-exchange", Encoding.UTF8.GetBytes(exchangeName));
            message.Headers.Add("x-first-death-reason", Encoding.UTF8.GetBytes(reason));

            // death info (count)
            var death = new Dictionary<string, object>
            {
                { "count", (long)0 }
            };

            message.Headers.Add("x-death", new List<object>() { death });

            return message;
        }
    }
}
