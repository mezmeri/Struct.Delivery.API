using Microsoft.Extensions.DependencyInjection;
using Delivery.Infrastructure.Persistence.Redis.Write;
using Delivery.Infrastructure.Persistence.CosmoDB.Read;
using Delivery.Application.Interfaces.Repositories;

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

            return services;
        }

        public static IServiceCollection ConfigureReadRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IProductReadRepository, ProductReadRepository>();

            return services;
        }
    }
}
