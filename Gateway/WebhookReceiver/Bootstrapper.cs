using Delivery.Composition;

namespace WebhookReceiver
{
    /// <summary>
    /// Responsible for setting up lifetimes for objects within the project.
    /// </summary>
    public class Bootstrapper
    {
        /// <summary>
        /// Configures the services from the Application layer.
        /// </summary>
        /// <param name="services">The service collection used by the <see cref="ServiceProvider"/></param>
        public void ConfigureApplicationDependencies(IServiceCollection services)
        {
            services.GetAllApplicationDependencies();
        }

        public void ConfigureInfrastructureDependencies(IServiceCollection services)
        {
            services.GetAllInfrastructureDependencies();
        }
    }
}
