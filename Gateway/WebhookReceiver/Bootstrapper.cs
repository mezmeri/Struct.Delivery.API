using Delivery.Composition;

namespace WebhookReceiver
{
    /// <summary>
    /// Responsible for setting up lifetimes for objects within the project.
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// Configures the services from the Application layer.
        /// </summary>
        /// <param name="services">The service collection used by the <see cref="ServiceProvider"/></param>
        public static IServiceCollection ConfigureApplicationDependencies(this IServiceCollection services)
        {
            return services.AddApplicationDependencies();
        }

        public static IServiceCollection ConfigureInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddInfrastructureDependencies(configuration);


        }


        //public static IServiceCollection ConfigureInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
        //{
        //    return services.AddInfrastructureDependencies();
        //    return configuration.


        //}
    }
}
