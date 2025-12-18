using Microsoft.Extensions.DependencyInjection;
using Delivery.Infrastructure.Persistence.Redis.Write;
using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Interfaces.Managers;
using Delivery.Infrastructure.Managers;
using Delivery.Infrastructure.Persistence.Redis.Read;

namespace Delivery.Infrastructure
{
    public static class DependencyRegistration
    {
        public static IServiceCollection ConfigureWriteRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IProductWriteRepository, ProductWriteRepository>();
            services.AddSingleton<IProductStructureWriteRepository, ProductStructureWriteRepository>();
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
            services.AddSingleton<IProductStructureReadRepository, ProductStructureReadRepository>();
            services.AddSingleton<IQueueReadRepository, QueueReadRepository>();

            return services;
        }
    }
}
