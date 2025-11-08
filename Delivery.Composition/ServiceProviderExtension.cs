using Microsoft.Extensions.DependencyInjection;
using Delivery.Application;

namespace Delivery.Composition
{
    public static class ServiceProviderExtension
    {
        public static IServiceCollection GetAllApplicationDependencies(this IServiceCollection services)
        {
            return services.GetAllApplicationServices();
        }

        public static IServiceCollection GetAllInfrastructureDependencies(this IServiceCollection services)
        {
            return services;
        }


    }
}
