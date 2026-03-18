using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers application layer services such as validators and MediatR
        /// behaviors.  This is intended to be called from the API startup.
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // register all FluentValidation validators in this assembly manually using reflection
            var assembly = typeof(DependencyInjection).Assembly;
            var validatorTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
                .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == typeof(IValidator<>));

            foreach (var v in validatorTypes)
            {
                services.AddTransient(v.Interface, v.Type);
            }

            // add validation pipeline behaviour so that every request is checked
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.ValidationBehavior<,>));

            return services;
        }
    }
}