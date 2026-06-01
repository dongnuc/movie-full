using MediatR;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Contract.Abstractions.Message
{

    public interface ICommandHander<TCommand> : IRequestHandler<TCommand, Result> where TCommand : ICommand
    {

    }

    public interface ICommandHander<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
        where TCommand : ICommand<TResponse>
    {

    }
}
