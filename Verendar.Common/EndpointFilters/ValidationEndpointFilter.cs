using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Verendar.Common.EndpointFilters
{
    public class ValidationEndpointFilter<T> : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var request = context.Arguments.OfType<T>().FirstOrDefault();

            if (request is null)
                return await next(context);

            var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
            if (validator is null)
                return await next(context);

            var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
            if (result.IsValid)
                return await next(context);

            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors);
        }
    }

    public static class ValidationEndpointFilter
    {
        public static IEndpointFilter Validate<T>() => new ValidationEndpointFilter<T>();
    }
}
