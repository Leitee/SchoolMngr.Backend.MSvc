using Fitnner.Infrastructure.Shared.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pandora.NetStdLibrary.Base.Abstractions.Desentralized;
using Pandora.NetStdLibrary.Base.Common;
using Pandora.NetStdLibrary.Base.Desentralized.EventBus;
using Pandora.NetStdLibrary.Base.Desentralized.EventBusRabbitMQ;
using Pandora.NetStdLibrary.Base.Tests;
using RabbitMQ.Client;
using System;

namespace Fitnner.Infrastructure.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureTier(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = BaseSettings.GetSettings(configuration ?? throw new ArgumentNullException(nameof(configuration)));

            services.AddDbContext<IntegrationEventLogContext>(op =>
            {
                op.EnableDetailedErrors(settings.IsDevelopment);
                op.EnableSensitiveDataLogging(settings.IsDevelopment);
                op.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                op.UseInMemoryDatabase("IntegrationEventLogDB");
            });

            services.AddIf(false, sc =>  sc.RegisterRabbitMQAsEventBus(configuration));
            services.AddIf(true, sc => sc.AddSingleton<IEventBus, EventBusDummy>());

            services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService>();
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            return services;
        }

        private static IServiceCollection RegisterRabbitMQAsEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                var factory = new ConnectionFactory()
                {
                    VirtualHost = "/",
                    HostName = configuration["EventBusConnection"],
                    Port = 5672,
                    DispatchConsumersAsync = true,
                    UserName = configuration["EventBusUserName"],
                    Password = configuration["EventBusPassword"]
                };

                return new DefaultRabbitMQPersistentConnection(factory, loggerFactory, 3);
            });

            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                return new EventBusRabbitMQ(rabbitMQPersistentConnection, loggerFactory, sp, eventBusSubcriptionsManager, "Fitnner_Queque", 3);
            });

            return services;
        }
    }
}
