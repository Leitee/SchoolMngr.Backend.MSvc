
namespace SchoolMngr.Infrastructure.Shared;

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
        services.AddIf(useEventBus, sc => sc.RegisterEventBus(infraSettings));
        services.AddIf(!useEventBus, sc => sc.AddSingleton<IEventBus, EventBusDummy>());

        services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService>();
        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

        return services;
    }

    private static IServiceCollection RegisterEventBus(this IServiceCollection services, INFRASettings settings)
    {
        services.AddIf(settings.IsDevelopment, sc =>
        {
            sc.AddSingleton<IRabbitMQPersistentConnection>(sp =>
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

                return new DefaultRabbitMQPersistentConnection(factory, loggerFactory, settings.EventBus.RetryCount);
            });

            sc.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                return new EventBusRabbitMQ(
                    rabbitMQPersistentConnection,
                    loggerFactory,
                    sp,
                    eventBusSubcriptionsManager,
                    settings.EventBus.QuequeName
                );
            });

            return sc;
        });

        services.AddIf(!settings.IsDevelopment, sc =>
        {
            sc.AddSingleton<IServiceBusPersisterConnection>(sp =>
            {
                var serviceBusConnectionString = settings.EventBus.Host;
                return new DefaultServiceBusPersisterConnection(serviceBusConnectionString);
            });

            sc.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
            {
                var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                var logger = sp.GetRequiredService<ILoggerFactory>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
                string subscriptionName = settings.EventBus.QuequeName;

                return new EventBusServiceBus(serviceBusPersisterConnection, logger,
                    eventBusSubcriptionsManager, subscriptionName, sp);
            });

            return sc;
        });

        return services;
    }
}
