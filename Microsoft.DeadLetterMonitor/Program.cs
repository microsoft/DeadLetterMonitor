using Microsoft.DeadLetterMonitor.Connectors;
using Microsoft.DeadLetterMonitor.Handlers;
using Microsoft.DeadLetterMonitor.Model;
using Microsoft.DeadLetterMonitor.Publishers;
using Microsoft.DeadLetterMonitor.Subscribers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace Microsoft.DeadLetterMonitor.EventProcessor {
    /// <summary>
    /// Main program.
    /// </summary>
    internal static class Program 
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>Host builder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
              .ConfigureAppConfiguration((hostContext, configBuilder) =>
              {
                  configBuilder.SetBasePath(Directory.GetCurrentDirectory());
                  configBuilder.AddJsonFile("appsettings.json", optional: true);
                  configBuilder.AddJsonFile(
                      $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                      optional: true);
                  configBuilder.AddUserSecrets<TimedHostedService>();
                  configBuilder.AddEnvironmentVariables();
              })
               .ConfigureServices((hostContext, services) =>
               {
                   services.AddApplicationInsightsTelemetryWorkerService();

                   services.AddSingleton<IConnection>(sp =>
                   {
                       return new Connectors.RabbitMQ.RabbitConnection(
                           hostContext.Configuration.GetValue<string>("RabbitMQ:NodesHostNames"),
                           hostContext.Configuration.GetValue<string>("RabbitMQ:VHost"),
                           hostContext.Configuration.GetValue<int>("RabbitMQ:Port"),
                           hostContext.Configuration.GetValue<string>("RabbitMQ:Username"),
                           hostContext.Configuration.GetValue<string>("RabbitMQ:Password"));
                   });

                   services.AddScoped<IHostedService, TimedHostedService>();
                   services.AddScoped<IGenericMessageHandler, GenericMessageHandler>();
                   services.AddScoped<IDelayedMessageHandler, DelayedMessageHandler>();
                   services.AddScoped<IGenericPublisher, GenericPublisher>();
                   services.AddScoped<IGenericSubscriber, GenericSubscriber>();
                   services.AddScoped<IDelayedSubscriber, DelayedSubscriber>();

                   services.Configure<DeadLetterMonitorOptions>(hostContext.Configuration.GetSection("DeadLetterMonitor"));
               });

            return builder;
        }
    }
}
