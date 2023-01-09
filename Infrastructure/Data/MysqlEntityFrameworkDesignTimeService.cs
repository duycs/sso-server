using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using MySql.EntityFrameworkCore.Extensions;

namespace AuthServer.Infrastructure.Data
{
    // To solved: Unable to resolve service for type 'Microsoft.EntityFrameworkCore.Storage.TypeMappingSourceDependencies' while attempting to activate 'MySql.EntityFrameworkCore.Storage.Internal.MySQLTypeMappingSource'.
    public class MysqlEntityFrameworkDesignTimeService : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkMySQL();
            new EntityFrameworkRelationalDesignServicesBuilder(serviceCollection)
                .TryAddCoreServices();
        }
    }
}