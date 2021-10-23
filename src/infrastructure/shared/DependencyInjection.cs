using SchoolMngr.Infrastructure.Shared.Configuration;
using SchoolMngr.Infrastructure.Shared.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Codeit.NetStdLibrary.Base.Abstractions.Desentralized;
using Codeit.NetStdLibrary.Base.Desentralized.EventBus;
using Codeit.NetStdLibrary.Base.Desentralized.EventBusRabbitMQ;
using Codeit.NetStdLibrary.Base.Tests;
using RabbitMQ.Client;
using System;

namespace SchoolMngr.Infrastructure.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, string sectionKey)
        {
            INFRASettings infraSettings;
            using (var servProv = services.BuildServiceProvider())
            {
                var config = servProv.GetService<IConfiguration>();
                infraSettings = config.GetSection(sectionKey).Get<INFRASettings>();
            }

            if (infraSettings is null)
                throw new ArgumentNullException(nameof(infraSettings));

            services.Configure<INFRASettings>(sp => sp = infraSettings);

            services.AddDbContext<IntegrationEventLogContext>(op =>
            {
                op.EnableDetailedErrors(infraSettings.EnableDetailedDebug);
                op.EnableSensitiveDataLogging(infraSettings.EnableDetailedDebug);
                op.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                op.UseInMemoryDatabase("IntegrationEventLogDB");
            });

            var useEventBus = infraSettings.EventBus.UseEventBus;
            services.AddIf(useEventBus, sc =>  sc.RegisterRabbitMQAsEventBus(infraSettings));
            services.AddIf(!useEventBus, sc => sc.AddSingleton<IEventBus, EventBusDummy>());

            services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService>();
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            return services;
        }

        private static IServiceCollection RegisterRabbitMQAsEventBus(this IServiceCollection services, INFRASettings settings)
        {
            var retryCount = settings.EventBus.RetryCount;

            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                var factory = new ConnectionFactory()
                {
                    VirtualHost = "/",
                    DispatchConsumersAsync = true,
                    HostName = settings.EventBus.Host,
                    Port = settings.EventBus.Port,
                    UserName = settings.EventBus.Username,
                    Password = settings.EventBus.Password
                };

                return new DefaultRabbitMQPersistentConnection(factory, loggerFactory, retryCount);
            });

            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                return new EventBusRabbitMQ(
                    rabbitMQPersistentConnection, 
                    loggerFactory, 
                    sp, 
                    eventBusSubcriptionsManager, 
                    settings.EventBus.QuequeName, 
                    retryCount
                );
            });

            return services;
        }
    }
}
