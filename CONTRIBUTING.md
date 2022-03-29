# DeadLetterMonitor Contribution Guide

The DeadLetterMonitor project welcomes contributions and suggestions.  

## DeadLetterMonitor GitHub Pages Site Contributions

Here's the general contribution process:

1. Fork this repo
1. Create a new branch
1. Commit your changes to that branch
1. Do a PR from your fork/branch to deadlettermonitor/master.


## Full Local Setup
Here's how to setup the project locally:

1. Start a local docker container with RabbitMQ (for a local RabbitMQ environment). You can use the following instruction "docker run --rm -it --hostname my-rabbit -p 15672:15672 -p 5672:5672 rabbitmq:3-management".

1. Open sln file in Visual Studio and build solution

1. Add a user secrets to add local configuration settings (default settings for the RabbitMQ)
```
{
  "RabbitMQ": {
    "HostName": "localhost", 
    "NodesHostNames": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  }
} 
```

## Create test scenario
How to configure local RabbitMQ with a real scenario

1. Create the following exchanges: "deadletter.exchange", "discard.exchange", "retry.exchange", "park.exchange"

2. Create a new queue "fast.expire.queue" with TTL = 1 and DeadLetterExchange = "deadletter.exchange"

3. Bing to this queue the exchanges: "discard.exchange", "retry.exchange", "park.exchange"

4. Create a queue "deadletter.queue" and bind to "deadletter.exchange"

Now you have a exchange for test the "discard", "retry", "park" rules. When you add any message to this exchanges, it will be delivered into the "fast.expire.queue" and sent to the "deadletter.exchange". 

The queue "deadletter.queue" gets the message and the DeadLetterMonitor - by default configured to watch this queue - will fire the rules. Just start the engine in debug and have fun! :D
