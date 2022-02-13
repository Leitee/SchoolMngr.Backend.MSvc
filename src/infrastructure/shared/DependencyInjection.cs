
namespace SchoolMngr.Infrastructure.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, string sectionKey)
    {
        INFRASettings? infraSettings;
        using (var servProv = services.BuildServiceProvider())
        {
            var config = servProv.GetRequiredService<IConfiguration>();
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
        ILoggerFactory? loggerFactory;
        IEventBusSubscriptionsManager? eventBusSubcriptionsManager;
        using var servProv = services.BuildServiceProvider();
        loggerFactory = servProv.GetRequiredService<ILoggerFactory>();
        eventBusSubcriptionsManager = servProv.GetRequiredService<IEventBusSubscriptionsManager>();

        services.AddIf(isDevEnvironment, sc =>
        {
            var factory = new ConnectionFactory()
                {
                    VirtualHost = "/",
                    DispatchConsumersAsync = true,
                    HostName = eventBusSection.Host,
                    Port = eventBusSection.Port,
                    UserName = eventBusSection.Username,
                    Password = eventBusSection.Password
                };

            var persistentConnection = new DefaultRabbitMQPersistentConnection(factory, loggerFactory, eventBusSection.RetryCount);
            sc.AddSingleton<IRabbitMQPersistentConnection>(persistentConnection);

            sc.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                return new EventBusRabbitMQ(
                    persistentConnection,
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
            var persistentConnection = new DefaultServiceBusPersisterConnection(eventBusSection.Host);
            sc.AddSingleton<IServiceBusPersisterConnection>(persistentConnection);

            sc.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
            {
                return new EventBusServiceBus(persistentConnection, loggerFactory,
                    eventBusSubcriptionsManager, eventBusSection.QuequeName, sp);
            });

            return sc;
        });

        return services;
    }
}
