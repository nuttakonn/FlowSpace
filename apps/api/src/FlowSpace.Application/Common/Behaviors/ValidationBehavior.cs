using FluentValidation;
using MediatR;
using FlowSpace.Application.Common.Models;

namespace FlowSpace.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationErrors = _validators
            .Select(validator => validator.Validate(context))
            .SelectMany(validationResult => validationResult.Errors)
            .Where(validationFailure => validationFailure is not null)
            .Select(failure => new Error(failure.ErrorCode, failure.ErrorMessage))
            .Distinct()
            .ToArray();

        if (validationErrors.Any())
        {
            // This is a bit tricky since we don't know if TResponse is Result or Result<TValue>
            // For now, we assume Result or Result<TValue> can be constructed from Error
            return CreateValidationResult<TResponse>(validationErrors);
        }

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(Error[] errors)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (ValidationResult.WithErrors(errors) as TResult)!;
        }

        var resultType = typeof(TResult).GetGenericArguments()[0];
        var validationResult = typeof(ValidationResult<>)
            .MakeGenericType(resultType)
            .GetMethod(nameof(ValidationResult.WithErrors))!
            .Invoke(null, new object?[] { errors });

        return (validationResult as TResult)!;
    }
}

public interface IValidationResult
{
    public static readonly Error ValidationError = new("ValidationError", "A validation failure occurred.");
    Error[] Errors { get; }
}

public class ValidationResult : Result, IValidationResult
{
    private ValidationResult(Error[] errors)
        : base(false, IValidationResult.ValidationError)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    public static ValidationResult WithErrors(Error[] errors) => new(errors);
}

public class ValidationResult<TValue> : Result<TValue>, IValidationResult
{
    private ValidationResult(Error[] errors)
        : base(default, false, IValidationResult.ValidationError)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    public static ValidationResult<TValue> WithErrors(Error[] errors) => new(errors);
}
