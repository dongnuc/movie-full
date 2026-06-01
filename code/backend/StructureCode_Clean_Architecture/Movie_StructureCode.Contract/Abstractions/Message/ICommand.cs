using MediatR;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Contract.Abstractions.Message
{
    // no return data just status
    public interface ICommand : IRequest<Result>
    {
    }


    // return data(Id movie)
    public interface ICommand<TResponse> : IRequest<Result<TResponse>>
    {

    }
}
