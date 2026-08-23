using FluentValidation;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Shared.Validations;

public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<T>().FirstOrDefault();

        if (request is null)
        {
            var problemError = Error.Problem("Validation.InvalidRequest", "Invalid request model structure.");
            return Result.Failure(problemError);
        }

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

        if (validator is not null)
        {
            var validationResult = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(failure => Result.Failure(Error.Validation(failure.PropertyName, failure.ErrorMessage)))
                    .ToArray();

                return ValidationError.FromResults(errors);
            }
        }

        return await next(context);
    }
}
