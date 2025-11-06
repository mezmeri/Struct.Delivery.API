using Delivery.Application;

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
        public void ConfigureServices(IServiceCollection services)
        {
            services.GetAllApplicationServices();
        }
    }
}
