using Microsoft.Extensions.DependencyInjection;
using Delivery.Application;
using Delivery.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Delivery.Composition
{
    public static class ServiceProviderExtension
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            return services.GetAllApplicationServices()
                .ConfigureManagers()
                .ConfigureEntityEventServices();
        }

        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            return services.ConfigureWriteRepositories()
                .ConfigureReadRepositories();
        }

        public static IServiceCollection AddClientDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            return services.ConfigureNotifiers(configuration);
        }
    }
}
