namespace WebhookReceiver
{
    /// <summary>
    /// Responsible for setting up lifetimes for objects within the project.
    /// </summary>
    public class Bootstrapper
    {
        /// <summary>
        /// Configures all the service classes for the service provider. All services will be transient instances, meaning they will be instantiated each time they are requested. This means that services should not hold any state. 
        /// </summary>
        /// <param name="services">The service collection used by the <see cref="ServiceProvider"/></param>
        public void ConfigureServices(IServiceCollection services)
        {
            IEnumerable<Type> appServices = GetType()
                .Assembly
                .GetTypes()
                .Where(x => x.Name.EndsWith("Service"))
                .Where(x => !x.IsAbstract && !x.IsInterface);

            foreach (Type? item in appServices)
            {
                services.AddTransient(item);
            }
        }

        /// <summary>
        /// Configures the lifetimes of all repositories. All repositories will be singleton, because they will be holding state.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureRepositories(IServiceCollection services)
        {
            IEnumerable<Type> appRepos = GetType()
                .Assembly
                .GetTypes()
                .Where(x => x.Name.EndsWith("Repository"))
                .Where(x => !x.IsAbstract && !x.IsInterface);

            foreach (Type? item in appRepos)
            {
                services.AddSingleton(item);
            }
        }
    }
}
