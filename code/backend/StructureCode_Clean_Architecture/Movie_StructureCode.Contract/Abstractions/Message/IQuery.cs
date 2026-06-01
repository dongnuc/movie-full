using MediatR;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Contract.Abstractions.Message
{

    public interface IQuery<TResponse> : IRequest<Result<TResponse>>
    {
    }
}
