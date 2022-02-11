
namespace SchoolMngr.Infrastructure.Shared.Configuration;

public abstract class DesignTimeDbContextFactoryBase<TContext> :
    IDesignTimeDbContextFactory<TContext> where TContext : DbContext
{
    private readonly IConfiguration _configuration;

    public DesignTimeDbContextFactoryBase(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TContext CreateDbContext(string[] args)
    {
        Console.WriteLine($"Creating instance of DesignTimeDbContextFactoryBase.CreatePersistenceBuilder.");
        var setting = _configuration.GetSection(DALSettings.SectionKey).Get<DALSettings>();
        Console.WriteLine($"Settings values:{JsonSerializer.Serialize(setting)}");
        var efPersistenceBuilder = CreatePersistenceBuilder(setting);

        Console.WriteLine($"DesignTimeDbContextFactoryBase.CreatePersistenceBuilder instance created.");
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        efPersistenceBuilder.BuildConfiguration(optionsBuilder);

        Console.WriteLine($"DesignTimeDbContextFactoryBase.CreateNewInstance.");
        return CreateNewInstance(optionsBuilder.Options);
    }

    protected abstract IPersistenceBuilder CreatePersistenceBuilder(DALSettings settings);
    protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);
}
