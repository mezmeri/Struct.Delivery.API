using Delivery.Application.Interfaces.Managers;
using Delivery.Application.Interfaces.Notifier;
using Delivery.Application.Interfaces.Repositories;
using Delivery.Infrastructure.Managers;
using Delivery.Infrastructure.Notifier;
using Delivery.Infrastructure.Persistence.Redis.Read;
using Delivery.Infrastructure.Persistence.Redis.Write;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Delivery.Infrastructure
{
    public static class DependencyRegistration
    {
        public static IServiceCollection ConfigureWriteRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IProductWriteRepository, ProductWriteRepository>();
            services.AddSingleton<IVariantWriteRepository, VariantWriteRepository>();
            services.AddSingleton<IVariantGroupWriteRepository, VariantGroupWriteRepository>();
            services.AddSingleton<ICatalogueWriteRepository, CatalogueWriteRepository>();
            services.AddSingleton<ICategoryWriteRepository, CategoryWriteRepository>();
            services.AddSingleton<IAttributeWriteRepository, AttributeWriteRepository>();
            services.AddSingleton<IAttributeScopeWriteRepository, AttributeScopeWriteRepository>();
            services.AddSingleton<IQueueWriteRepository, QueueWriteRepository>();

            return services;
        }

        public static IServiceCollection ConfigureReadRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IProductReadRepository, ProductReadRepository>();
            services.AddSingleton<IQueueReadRepository, QueueReadRepository>();
            services.AddSingleton<IVariantReadRepository, VariantReadRepository>();

            return services;
        }
    }
}
