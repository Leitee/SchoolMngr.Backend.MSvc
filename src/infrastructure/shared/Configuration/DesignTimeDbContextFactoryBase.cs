
namespace SchoolMngr.Infrastructure.Shared.Configuration
{
    using Codeit.NetStdLibrary.Base.Abstractions.DataAccess;
    using Codeit.NetStdLibrary.Base.DataAccess;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using System;

    public abstract class DesignTimeDbContextFactoryBase<TContext> :
        IDesignTimeDbContextFactory<TContext> where TContext : DbContext
    {
        private readonly DALSettings _settings;

        public DesignTimeDbContextFactoryBase(DALSettings settings)
        {
            _settings = settings;
        }

        public TContext CreateDbContext(string[] args)
        {
            Console.WriteLine($"Arguments passed: {string.Join(",", args)}");

            Console.WriteLine($"Creating instance of DesignTimeDbContextFactoryBase.CreatePersistenceBuilder.");
            var efPersistenceBuilder = CreatePersistenceBuilder(_settings);
            Console.WriteLine($"DesignTimeDbContextFactoryBase.CreatePersistenceBuilder instance created.");
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            efPersistenceBuilder.BuildConfiguration(optionsBuilder);

            Console.WriteLine($"DesignTimeDbContextFactoryBase.CreateNewInstance.");
            return CreateNewInstance(optionsBuilder.Options);
        }

        protected abstract IPersistenceBuilder CreatePersistenceBuilder(DALSettings settings);
        protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);
    }
}
