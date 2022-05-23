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
                Discard = "topic1,type1,reason1;*,type2,reason2;topic3,*,reason3; topic4,type4,*;topic5,*,*;*,type6,*;*,*,reason7",
                Park = "topic8,type8,reason8",
                Retry = "topic9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var body = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

            var message = new Message("1", DateTime.UtcNow.ToString(), "type1", "topic1", "key", null, new Dictionary<string, object>(), body);

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
                Discard = "topic1,type1,reason1;*,type2,reason2;topic3,*,reason3; topic4,type4,*;topic5,*,*;*,type6,*;*,*,reason7",
                Park = "topic8,type8,reason8",
                Retry = "topic9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var body = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

            var message = new Message("1", DateTime.UtcNow.ToString(), "type1", "topic1", "key", null, new Dictionary<string, object>(), body);

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
                Discard = "topic1,type1,reason1;*,type2,reason2;topic3,*,reason3; topic4,type4,*;topic5,*,*;*,type6,*;*,*,reason7",
                Park = "topic8,type8,reason8",
                Retry = "topic9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var body = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

            var message = new Message("1", DateTime.UtcNow.ToString(), "type1", "topic1", "key", null, new Dictionary<string, object>(), body);

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
                Discard = "topic1,type1,reason1;*,type2,reason2;topic3,*,reason3; topic4,type4,*;topic5,*,*;*,type6,*;*,*,reason7",
                Park = "topic8,type8,reason8",
                Retry = "topic9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { MaxRetries = 2, Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var msg1 = GetMessageArgs("topic1", "type1", "reason1");
            var msg2 = GetMessageArgs("topic999", "type2", "reason2");
            var msg3 = GetMessageArgs("topic3", "type999", "reason3");
            var msg4 = GetMessageArgs("topic4", "type4", "reason999");
            var msg5 = GetMessageArgs("topic5", "type999", "reason999");
            var msg6 = GetMessageArgs("topic999", "type6", "reason999");
            var msg7 = GetMessageArgs("topic999", "type999", "reason7");

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
                Retry = "topic1,type1,reason1;*,type2,reason2;topic3,*,reason3; topic4,type4,*;topic5,*,*;*,type6,*;*,*,reason7",
                Park = "topic8,type8,reason8",
                Discard = "topic9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { DelayedTopicName = "delayedexhange", MaxRetries = 2, Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var msg1 = GetMessageArgs("topic1", "type1", "reason1");
            var msg2 = GetMessageArgs("topic999", "type2", "reason2");
            var msg3 = GetMessageArgs("topic3", "type999", "reason3");
            var msg4 = GetMessageArgs("topic4", "type4", "reason999");
            var msg5 = GetMessageArgs("topic5", "type999", "reason999");
            var msg6 = GetMessageArgs("topic999", "type6", "reason999");
            var msg7 = GetMessageArgs("topic999", "type999", "reason7");

            // Execute 
            handler.HandleMessage(msg1);
            handler.HandleMessage(msg2);
            handler.HandleMessage(msg3);
            handler.HandleMessage(msg4);
            handler.HandleMessage(msg5);
            handler.HandleMessage(msg6);
            handler.HandleMessage(msg7);

            // Check all messages are published to the delayed topic - all retried
            Assert.AreEqual(7, mockPublisher.PublishedMessages.Count(m => m == options.DelayedTopicName));
        }

        /// <summary>
        /// Test handle message for park rules.
        /// </summary>
        [TestMethod]
        public void TestHandleMessageForPark()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Park = "topic1,type1,reason1;*,type2,reason2;topic3,*,reason3; topic4,type4,*;topic5,*,*;*,type6,*;*,*,reason7",
                Retry = "topic8,type8,reason8",
                Discard = "topic9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { ParkingLotTopicName = "pakinglot.topic", MaxRetries = 2, Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var msg1 = GetMessageArgs("topic1", "type1", "reason1");
            var msg2 = GetMessageArgs("topic999", "type2", "reason2");
            var msg3 = GetMessageArgs("topic3", "type999", "reason3");
            var msg4 = GetMessageArgs("topic4", "type4", "reason999");
            var msg5 = GetMessageArgs("topic5", "type999", "reason999");
            var msg6 = GetMessageArgs("topic999", "type6", "reason999");
            var msg7 = GetMessageArgs("topic999", "type999", "reason7");

            // Execute 
            handler.HandleMessage(msg1);
            handler.HandleMessage(msg2);
            handler.HandleMessage(msg3);
            handler.HandleMessage(msg4);
            handler.HandleMessage(msg5);
            handler.HandleMessage(msg6);
            handler.HandleMessage(msg7);

            // Check all messages are published to the delayed topic - all retried
            Assert.AreEqual(7, mockPublisher.PublishedMessages.Count(m => m == options.ParkingLotTopicName));
        }

        /// <summary>
        /// Test handle message for max retries rules.
        /// </summary>
        [TestMethod]
        public void TestHandleMessageMaxRetries()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Retry = "topic1,type1,reason1;*,type2,reason2;topic3,*,reason3; topic4,type4,*;topic5,*,*;*,type6,*;*,*,reason7",
                Park = "topic8,type8,reason8",
                Discard = "topic9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { ParkingLotTopicName = "pakinglot.topic", MaxRetries = 2, Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var msg1 = GetMessageArgs("topic1", "type1", "reason1", options.MaxRetries);

            // Execute 1
            handler.HandleMessage(msg1);

            // Check message is published to the parking lot topic - max retries reached
            Assert.AreEqual(1, mockPublisher.PublishedMessages.Count(m => m == options.ParkingLotTopicName));
        }

        /// <summary>
        /// Test handle message for default behaviour rules.
        /// </summary>
        [TestMethod]
        public void TestHandleMessageDefaultBehaviour()
        {
            var rulesOptions = new DeadLetterMonitorRules
            {
                Park = "topic1,type1,reason1;*,type2,reason2;topic3,*,reason3; topic4,type4,*;topic5,*,*;*,type6,*;*,*,reason7",
                Retry = "topic8,type8,reason8",
                Discard = "topic9,type9,reason9"
            };

            var mockAppInsights = new TelemetryClient(new TelemetryConfiguration());

            DeadLetterMonitorOptions options = new DeadLetterMonitorOptions() { ParkingLotTopicName = "pakinglot.topic", MaxRetries = 2, Rules = rulesOptions };

            var mockOptions = new Mock<IOptions<DeadLetterMonitorOptions>>();
            mockOptions.Setup(c => c.Value).Returns(options);

            var mockPublisher = new GenericPublisherMock();

            var handler = new GenericMessageHandler(mockOptions.Object, mockPublisher, mockAppInsights);

            var msg1 = GetMessageArgs("topic999", "type999", "reason999");

            // Execute 
            handler.HandleMessage(msg1);

            // Check all messages are published to the delayed topic - all retried
            Assert.AreEqual(1, mockPublisher.PublishedMessages.Count(m => m == options.ParkingLotTopicName));
        }

        private IMessage GetMessageArgs(string topicName, string type, string reason, int retries = 0)
        {
            var body = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

            var message = new Message("1", DateTime.UtcNow.ToString(), type, "topic1", "key", null, new Dictionary<string, object>(), body);

            message.FirstDeathTopic = topicName;
            message.FirstDeathReason = reason;
            message.DeathCount = retries;

            return message;
        }
    }
}
