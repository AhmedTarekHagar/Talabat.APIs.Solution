using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Talabat.APIs.Errors;
using Talabat.APIs.Helpers;
using Talabat.Core.IRepositories;
using Talabat.Core.IServices;
using Talabat.Core.Services;
using Talabat.Repository;
using Talabat.Service;

namespace Talabat.APIs.Extentions
{
    public static class ApplicationServicesExtentions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IOrderService, OrderService>();

            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<IPaymentService, PaymentService>();

            services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddAutoMapper(typeof(MappingProfiles));

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState
                                 .Where(M => M.Value.Errors.Count() > 0)
                                 .SelectMany(M => M.Value.Errors)
                                 .Select(E => E.ErrorMessage)
                                 .ToArray();
                    var validationErrorResponse = new ApiValidationErrorResponse()
                    {
                        Errors = errors
                    };
                    return new BadRequestObjectResult(validationErrorResponse);
                };
            });

            return services;
        }
    }
}
