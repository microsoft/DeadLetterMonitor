# Introduction 

DeadLetterMonitor is a tool that automatically deals, according to custom rules, with some errors that can occur in Pub/Sub Architectures.

Things like retry after an unexpected down time on the consumer, reference data not being properly set in the destination service, configuration missing, etc. 

This project implements a simple Dead Letter Monitor service that listens on all configured dead letter queues and executes a Retry pattern and a Circuit breaker pattern.

It implements a set of simple rules that allows definition of wich messages should be retried, for how many times, and the delay between retries.
All the messages that are not retried will be deleted or moved to a "Parking lot" storage to be manually processed.

![Design](/images/design.png)

# Message handling and delay retries
All dead lettered messages processed by the monitor engine are handled by a generic handler that, accordingly to a set of rules, will discard, retry or send the message to a parking lot.

## Discard
All messages configured to be discarded will just be deleted from the queue.

## Retry
All messages configured to be retried will be sent to the original exchange/topic, to be reprocessed by the original subscribers. This retry will be executed using a delay to ensure that temporary problems in the infrastructure are already fixed.

**Delayed messages**: 
To create the delay, all messages to retry will be posted on a `delayed exchange and queue`, that is just a queue with a TTL setting.

When the delayed message TTL expires the message will be automatically sent to a `Delayed Deadletter exchange and queue` that has a specific handler that immediatly sends the message to the original queue.

# Configuration
All configurations are stored in `appsettings.json` file.

```
 "DeadLetterMonitor": {
    "MaxRetries": 2,
    "DeadLetterQueues": "deadletter.queue",
    "DelayedExchangeName": "delayed.exchange",
    "DelayedQueueName": "delayed.queue",
    "DelayedDeadLetterExchangeName": "delayed.deadletter.exchange",
    "DelayedDeadLetterQueueName": "delayed.deadletter.queue",
    "ParkingLotExchangeName": "parkinglot.exchange",
    "ParkingLotQueueName": "parkinglot.queue",
    "DelayValue": 10000,
    "Rules": {
      "Discard": "work.exchange1,Namespace.Abstractions.GenericEvent1,rejected1",
      "Retry": "work.exchange,Namespace.Abstractions.GenericEvent,rejected",
      "Park": "work.exchange1,Namespace.Abstractions.GenericEvent1,rejected1"
    }
  }
```

| Property | Description |
| ----------- | ----------- |
| MaxRetries | Maximum number of retries per message |
| DeadLetterQueues | Dead letter queues names, separated by comma |
| DelayedExchangeName | Delayed Exchange name - the name of the exchange to handle delayed messages |
| DelayedQueueName | Delayed Queue name - the name of the queue to receive delayed messages |
| DelayedDeadLetterExchangeName | Delayed dead letter Exchange name - the name of the exchange to handle delayed messages |
| DelayedDeadLetterQueueName | Delayed dead letter Queue name - the name of the queue to receive delayed messages |
| DelayValue | Time in second of delay before sending the message back for retry. |
| ParkingLotExchangeName | Parking lot exchange name |
| ParkingLotQueueName | Parking lot queue name |
| Rules | List of rules to implement in monitor engine. Each rule is applied using a fliter composed by a list of `OriginalExchange` a `MessageType` and a `DeathReason`. The list of filters is separated by comma. (ex. `work.exchange1,Namespace.Abstractions.GenericEvent1,rejected1` - this filter will apply the rule to a message from Exchange `work.exchange1` with Message Type of `Namespace.Abstractions` and a DeathReason of `rejected1`) |
| Rules: Discard | List of message filters of messages to be discarded. The messages that match this filter criteria are automatically deleted. |
| Rules: Retry | List of message filters of messages to be retried until the `MaxRetries` is reached. The messages that match this filter criteria are sent for retry. |
| Rules: Park | List of message filters of messages to be Parked directly. The messages that match this filter criteria are sent to the parking lot. |

# Contribute
This project welcomes contributions and suggestions. 

Please see our [contributing guide](CONTRIBUTING.md) for complete instructions on how you can contribute to the DeadLetterMonitor. 

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
