using FluentValidation;
using MediatR;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Behaviors
{
    public sealed class ValidationPipelineBehavior<TRequest, TResponse> : 
        IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse> where TResponse : Result
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

        public async Task<TResponse> Handle(TRequest request, 
            RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if(!_validators.Any())
            {
                return await next();
            }

            Error[] error = _validators.Select(validaitor => validaitor.Validate(request))
                .SelectMany(validaitorResult => validaitorResult.Errors)
                .Where(validaitorError => validaitorError is not null)
                .Select(failure => new Error(failure.PropertyName, failure.ErrorMessage)).Distinct().ToArray();


            if (error.Any())
            {
                return CreateValidationResult<TResponse>(error);
            }

            return await next();



        }

        private static TResult CreateValidationResult<TResult>(Error[] error) where TResult : Result
        {
            if(typeof(TResult) == typeof(Result))
            {
                return (ValidationResult.WithErrors(error) as TResult)!;
            }

            object validationResult = typeof(ValidationResult<>)
                .MakeGenericType(typeof(TResult).GenericTypeArguments[0])
                .GetMethod(nameof(ValidationResult<object>.WithErrors))!
                .Invoke(null, new object?[] { error })!;
        
            return (TResult)validationResult;
        }
    }
}
