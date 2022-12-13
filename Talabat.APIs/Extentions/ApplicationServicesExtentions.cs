using Microsoft.Extensions.DependencyInjection;
using Talabat.APIs.Helpers;
using Talabat.Core.IRepositories;
using Talabat.Repository;

namespace Talabat.APIs.Extentions
{
    public static class ApplicationServicesExtentions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddAutoMapper(typeof(MappingProfiles));

            return services;
        }
    }
}
