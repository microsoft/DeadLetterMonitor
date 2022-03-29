using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.DeadLetterMonitor.Handlers;
using Microsoft.DeadLetterMonitor.Model;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.DeadLetterMonitor.Tests.Handlers {
    [TestClass]
    public class GenericMessageHandlerTest 
    {
        /// <summary>
        /// Test sending message without its headers.
        /// </summary>
        [TestMethod]
        public void TestHandleMessageWithoutFirstHeaders()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Discard = "exchange1,type1,reason1;*,type2,reason2;exchange3,*,reason3; exchange4,type4,*;exchange5,*,*;*,type6,*;*,*,reason7",
                Park = "exchange8,type8,reason8",
                Retry = "exchange9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var body = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

            var message = new Message("1", DateTime.UtcNow.ToString(), "type1", "exchange1", "key", null, new Dictionary<string, object>(), body);

            Assert.ThrowsException<ArgumentException>(() => handler.HandleMessage(message));
        }

        /// <summary>
        /// Test handle message without the x-death header.
        /// </summary>
        [TestMethod]
        public void TestHandleMessageWithoutDeathHeader()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Discard = "exchange1,type1,reason1;*,type2,reason2;exchange3,*,reason3; exchange4,type4,*;exchange5,*,*;*,type6,*;*,*,reason7",
                Park = "exchange8,type8,reason8",
                Retry = "exchange9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var body = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

            var message = new Message("1", DateTime.UtcNow.ToString(), "type1", "exchange1", "key", null, new Dictionary<string, object>(), body);

            message.Headers.Add("x-first-death-exchange", Encoding.UTF8.GetBytes("dummy"));
            message.Headers.Add("x-first-death-reason", Encoding.UTF8.GetBytes("dummy"));

            Assert.ThrowsException<ArgumentException>(() => handler.HandleMessage(message));
        }

        /// <summary>
        /// Test handle message without the death count.
        /// </summary>
        [TestMethod]
        public void TestHandleMessageWithoutDeathCount()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Discard = "exchange1,type1,reason1;*,type2,reason2;exchange3,*,reason3; exchange4,type4,*;exchange5,*,*;*,type6,*;*,*,reason7",
                Park = "exchange8,type8,reason8",
                Retry = "exchange9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var body = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

            var message = new Message("1", DateTime.UtcNow.ToString(), "type1", "exchange1", "key", null, new Dictionary<string, object>(), body);

            message.Headers.Add("x-first-death-exchange", Encoding.UTF8.GetBytes("dummy"));
            message.Headers.Add("x-first-death-reason", Encoding.UTF8.GetBytes("dummy"));
            message.Headers.Add("x-death", null);

            Assert.ThrowsException<ArgumentException>(() => handler.HandleMessage(message));
        }

        /// <summary>
        /// Test handle message for discard rules.
        /// </summary>
        [TestMethod]
        public void TestHandleMessageForDiscard()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Discard = "exchange1,type1,reason1;*,type2,reason2;exchange3,*,reason3; exchange4,type4,*;exchange5,*,*;*,type6,*;*,*,reason7",
                Park = "exchange8,type8,reason8",
                Retry = "exchange9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { MaxRetries = 2, Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var msg1 = GetMessageArgs("exchange1", "type1", "reason1");
            var msg2 = GetMessageArgs("exchange999", "type2", "reason2");
            var msg3 = GetMessageArgs("exchange3", "type999", "reason3");
            var msg4 = GetMessageArgs("exchange4", "type4", "reason999");
            var msg5 = GetMessageArgs("exchange5", "type999", "reason999");
            var msg6 = GetMessageArgs("exchange999", "type6", "reason999");
            var msg7 = GetMessageArgs("exchange999", "type999", "reason7");

            // Execute 
            handler.HandleMessage(msg1);
            handler.HandleMessage(msg2);
            handler.HandleMessage(msg3);
            handler.HandleMessage(msg4);
            handler.HandleMessage(msg5);
            handler.HandleMessage(msg6);
            handler.HandleMessage(msg7);

            // Check if no messages are published - all discarded
            Assert.AreEqual(0, mockPublisher.PublishedMessages.Count);
        }

        /// <summary>
        /// Test handle message for retry rules.
        /// </summary>
        [TestMethod]
        public void TestHandleMessageForRetry()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Retry = "exchange1,type1,reason1;*,type2,reason2;exchange3,*,reason3; exchange4,type4,*;exchange5,*,*;*,type6,*;*,*,reason7",
                Park = "exchange8,type8,reason8",
                Discard = "exchange9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { DelayedExchangeName = "delayedexhange", MaxRetries = 2, Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var msg1 = GetMessageArgs("exchange1", "type1", "reason1");
            var msg2 = GetMessageArgs("exchange999", "type2", "reason2");
            var msg3 = GetMessageArgs("exchange3", "type999", "reason3");
            var msg4 = GetMessageArgs("exchange4", "type4", "reason999");
            var msg5 = GetMessageArgs("exchange5", "type999", "reason999");
            var msg6 = GetMessageArgs("exchange999", "type6", "reason999");
            var msg7 = GetMessageArgs("exchange999", "type999", "reason7");

            // Execute 
            handler.HandleMessage(msg1);
            handler.HandleMessage(msg2);
            handler.HandleMessage(msg3);
            handler.HandleMessage(msg4);
            handler.HandleMessage(msg5);
            handler.HandleMessage(msg6);
            handler.HandleMessage(msg7);

            // Check all messages are published to the delayed exchange - all retried
            Assert.AreEqual(7, mockPublisher.PublishedMessages.Count(m => m == options.DelayedExchangeName));
        }

        /// <summary>
        /// Test handle message for park rules.
        /// </summary>
        [TestMethod]
        public void TestHandleMessageForPark()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Park = "exchange1,type1,reason1;*,type2,reason2;exchange3,*,reason3; exchange4,type4,*;exchange5,*,*;*,type6,*;*,*,reason7",
                Retry = "exchange8,type8,reason8",
                Discard = "exchange9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { ParkingLotExchangeName = "pakinglotexhange", MaxRetries = 2, Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var msg1 = GetMessageArgs("exchange1", "type1", "reason1");
            var msg2 = GetMessageArgs("exchange999", "type2", "reason2");
            var msg3 = GetMessageArgs("exchange3", "type999", "reason3");
            var msg4 = GetMessageArgs("exchange4", "type4", "reason999");
            var msg5 = GetMessageArgs("exchange5", "type999", "reason999");
            var msg6 = GetMessageArgs("exchange999", "type6", "reason999");
            var msg7 = GetMessageArgs("exchange999", "type999", "reason7");

            // Execute 
            handler.HandleMessage(msg1);
            handler.HandleMessage(msg2);
            handler.HandleMessage(msg3);
            handler.HandleMessage(msg4);
            handler.HandleMessage(msg5);
            handler.HandleMessage(msg6);
            handler.HandleMessage(msg7);

            // Check all messages are published to the delayed exchange - all retried
            Assert.AreEqual(7, mockPublisher.PublishedMessages.Count(m => m == options.ParkingLotExchangeName));
        }

        /// <summary>
        /// Test handle message for max retries rules.
        /// </summary>
        [TestMethod]
        public void TestHandleMessageMaxRetries()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Retry = "exchange1,type1,reason1;*,type2,reason2;exchange3,*,reason3; exchange4,type4,*;exchange5,*,*;*,type6,*;*,*,reason7",
                Park = "exchange8,type8,reason8",
                Discard = "exchange9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { ParkingLotExchangeName = "pakinglotexhange", MaxRetries = 2, Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var msg1 = GetMessageArgs("exchange1", "type1", "reason1", options.MaxRetries);

            // Execute 1
            handler.HandleMessage(msg1);

            // Check message is published to the parking lot exchange - max retries reached
            Assert.AreEqual(1, mockPublisher.PublishedMessages.Count(m => m == options.ParkingLotExchangeName));
        }

        /// <summary>
        /// Test handle message for default behaviour rules.
        /// </summary>
        [TestMethod]
        public void TestHandleMessageDefaultBehaviour()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Park = "exchange1,type1,reason1;*,type2,reason2;exchange3,*,reason3; exchange4,type4,*;exchange5,*,*;*,type6,*;*,*,reason7",
                Retry = "exchange8,type8,reason8",
                Discard = "exchange9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { ParkingLotExchangeName = "pakinglotexhange", MaxRetries = 2, Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var msg1 = GetMessageArgs("exchange999", "type999", "reason999");

            // Execute 
            handler.HandleMessage(msg1);

            // Check all messages are published to the delayed exchange - all retried
            Assert.AreEqual(1, mockPublisher.PublishedMessages.Count(m => m == options.ParkingLotExchangeName));
        }

        private IMessage GetMessageArgs(string exchangeName, string type, string reason, long retries = 0)
        {
            var body = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

            var message = new Message("1", DateTime.UtcNow.ToString(), type, "exchange1", "key", null, new Dictionary<string, object>(), body);

            message.Headers.Add("x-first-death-exchange", Encoding.UTF8.GetBytes(exchangeName));
            message.Headers.Add("x-first-death-reason", Encoding.UTF8.GetBytes(reason));

            // death info (count)
            var death = new Dictionary<string, object>
            {
                { "count", retries }
            };

            message.Headers.Add("x-death", new List<object>() { death });

            return message;
        }
    }
}
