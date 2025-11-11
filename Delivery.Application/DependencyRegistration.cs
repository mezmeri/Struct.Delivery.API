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

        /// <summary>
        /// Returns all the interfaces within the application layer.
        /// </summary>
        /// <param name="interfaces"></param>
        /// <returns></returns>
        public static IServiceCollection GetAllApplicationInterfaces(this IServiceCollection services)
        {
            IEnumerable<Type> applicationInterfaces = Assembly.GetAssembly(typeof(ServiceRegistration))
                .GetTypes()
                .Where(x => x.Name.StartsWith("I"))
                .Where(x => x.IsAbstract && x.IsInterface);

            foreach (Type item in applicationInterfaces)
            {
                services.AddTransient(item);
            }

            return services;
        }
    }
}
