using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Codeit.NetStdLibrary.Base.Abstractions.DataAccess;
using System;

namespace SchoolMngr.Infrastructure.Shared.Configuration
{
    public abstract class DesignTimeDbContextFactoryBase<TContext> :
        IDesignTimeDbContextFactory<TContext> where TContext : DbContext
    {
        public TContext CreateDbContext(string[] args)
        {
            Console.WriteLine($"Arguments passed: {string.Join(",", args)}");
            var configuration = SharedHostConfiguration.GetBasicConfiguration();

            Console.WriteLine($"DesignTimeDbContextFactoryBase.CreatePersistenceBuilder.");
            var efPersistenceBuilder = CreatePersistenceBuilder(configuration);
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            efPersistenceBuilder.ConfigurePersistence(optionsBuilder);

            Console.WriteLine($"DesignTimeDbContextFactoryBase.CreateNewInstance.");
            return CreateNewInstance(optionsBuilder.Options);
        }

        protected abstract IPersistenceBuider CreatePersistenceBuilder(IConfiguration configuration);
        protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);
    }
}
