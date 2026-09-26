using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NieFarm.Application.Common.Behaviors;
using NieFarm.Application.Features.Carts;

namespace NieFarm.Application.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Order matters: logging wraps validation, so a request rejected by a validator is
        // still recorded with its outcome and timing.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<CartAssembler>();

        return services;
    }
}
