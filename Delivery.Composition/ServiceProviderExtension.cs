using Microsoft.Extensions.DependencyInjection;
using Delivery.Application;
using Delivery.Infrastructure;

namespace Delivery.Composition
{
    public static class ServiceProviderExtension
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            return services.GetAllApplicationServices();
        }

        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            return services.ConfigureWriteRepositories()
                .ConfigureReadRepositories()
                .ConfigureManagers();
        }
    }
}
