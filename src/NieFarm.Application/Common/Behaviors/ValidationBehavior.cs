using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace NieFarm.Application.Common.Behaviors;

/// <summary>
/// Runs all FluentValidation validators for a request before it reaches its handler.
/// When the response is an Ardalis.Result (or Result&lt;T&gt;), failures are returned as an
/// invalid result so the UI can display them; otherwise a ValidationException is thrown.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        if (TryBuildInvalidResult(failures, out var invalid))
            return invalid!;

        throw new ValidationException(failures);
    }

    private static bool TryBuildInvalidResult(
        IEnumerable<FluentValidation.Results.ValidationFailure> failures, out TResponse? response)
    {
        response = default;

        // Only Ardalis.Result-shaped responses can carry validation errors.
        if (!typeof(Ardalis.Result.IResult).IsAssignableFrom(typeof(TResponse)))
            return false;

        var validationErrors = failures
            .Select(f => new ValidationError
            {
                Identifier = f.PropertyName,
                ErrorMessage = f.ErrorMessage,
                Severity = ValidationSeverity.Error
            })
            .ToArray();

        var invalidMethod = typeof(TResponse).GetMethod(
            nameof(Result.Invalid), [typeof(ValidationError[])]);

        if (invalidMethod is null)
            return false;

        response = (TResponse)invalidMethod.Invoke(null, [validationErrors])!;
        return true;
    }
}
