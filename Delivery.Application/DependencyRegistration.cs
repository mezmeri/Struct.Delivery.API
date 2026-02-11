using Delivery.Application.Interfaces;
using Delivery.Application.Interfaces.Managers;
using Delivery.Application.Services.EntityEventServices;
using Delivery.Infrastructure.Managers;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Delivery.Application
{
    /// <summary>
    /// Holds extension methods related to the service provider. 
    /// </summary>
    public static class DependencyRegistration
    {
        /// <summary>
        /// Returns all the services within the application layer.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection GetAllApplicationServices(this IServiceCollection services)
        {
            IEnumerable<Type> applicationServices = Assembly.GetAssembly(typeof(DependencyRegistration))
                .GetTypes()
                .Where(x => x.Name.EndsWith("Service"))
                .Where(x => !x.IsAbstract && !x.IsInterface);

            foreach (Type item in applicationServices)
            {
                services.AddTransient(item);
            }

            return services;
        }

        public static IServiceCollection ConfigureManagers(this IServiceCollection services)
        {
            services.AddSingleton<IQueueManager, QueueManager>();

            return services;
        }

        public static IServiceCollection ConfigureEntityEventServices(this IServiceCollection services)
        {
            services.AddTransient<IEntityEventService, ProductEventService>();
            services.AddTransient<IEntityEventService, VariantEventService>();

            return services;
        }
    }
}
