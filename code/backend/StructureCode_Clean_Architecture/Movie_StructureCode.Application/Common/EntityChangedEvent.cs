using MediatR;

namespace Movie_StructureCode.Application.Common
{
    public class EntityChangedEvent : INotification
    {
        public string Entity { get; }
        public Guid? EntityId { get; }
        public EntityChangedEvent(string entity, Guid? entityId)
        {
            Entity = entity;
            EntityId = entityId;
        }
    }
}
