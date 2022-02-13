
namespace SchoolMngr.Infrastructure.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, string sectionKey)
    {
        INFRASettings? infraSettings;
        using (var servProv = services.BuildServiceProvider())
        {
            var config = servProv.GetService<IConfiguration>();
            infraSettings = config?.GetSection(sectionKey).Get<INFRASettings>();
        }

        if (infraSettings?.EventBus is null)
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
        services.AddIf(useEventBus is true, sc => sc.RegisterEventBus(infraSettings.EventBus, infraSettings.IsDevelopment));
        services.AddIf(!useEventBus is true, sc => sc.AddSingleton<IEventBus, EventBusDummy>());

        services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService>();
        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

        return services;
    }

    private static IServiceCollection RegisterEventBus(this IServiceCollection services, EventBusSection eventBusSection, bool isDevEnvironment)
    {
        services.AddIf(isDevEnvironment, sc =>
        {
            sc.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                var factory = new ConnectionFactory()
                {
                    VirtualHost = "/",
                    DispatchConsumersAsync = true,
                    HostName = eventBusSection.Host,
                    Port = eventBusSection.Port,
                    UserName = eventBusSection.Username,
                    Password = eventBusSection.Password
                };

                return new DefaultRabbitMQPersistentConnection(factory, loggerFactory, eventBusSection.RetryCount);
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
                    eventBusSection.QuequeName
                );
            });

            return sc;
        });

        services.AddIf(!isDevEnvironment, sc =>
        {
            sc.AddSingleton<IServiceBusPersisterConnection>(sp =>
            {
                return new DefaultServiceBusPersisterConnection(eventBusSection.Host);
            });

            sc.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
            {
                var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                return new EventBusServiceBus(serviceBusPersisterConnection, loggerFactory,
                    eventBusSubcriptionsManager, eventBusSection.QuequeName, sp);
            });

            return sc;
        });

        return services;
    }
}
