using MediatR;

namespace Movie_StructureCode.Contract.Abstractions.Message
{
    public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent> where TEvent: IDomainEvent 
    {
    }
}
