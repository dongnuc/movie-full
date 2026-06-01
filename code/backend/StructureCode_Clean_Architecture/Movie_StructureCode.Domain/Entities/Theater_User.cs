using System;

namespace Movie_StructureCode.Domain.Entities
{
    public class Theater_User : BaseEntities
    {
        public Guid UserId { get; private set; }
        public AppUser? User { get; private set; }

        public Guid TheaterId { get; private set; }
        public Theater? Theater { get; private set; }

        public int IsPrimary { get; private set; }

        // Parameterless constructor for EF Core
        private Theater_User() : base() { }

        private Theater_User(Guid id, Guid userId, Guid theaterId, int isPrimary, bool isActive, DateTime dateCreate, DateTime dateUpdate)
            : base(id, dateCreate, dateUpdate, isActive)
        {
            UserId = userId;
            TheaterId = theaterId;
            IsPrimary = isPrimary;
        }

        public static Theater_User Create(Guid id, Guid userId, Guid theaterId, int isPrimary, bool isActive = true)
        {
            return new Theater_User(
                id,
                userId,
                theaterId,
                isPrimary,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public void UpdateDetails(Guid userId, Guid theaterId, int isPrimary, bool isActive)
        {
            UserId = userId;
            TheaterId = theaterId;
            IsPrimary = isPrimary;
            IsActive = isActive;
            DateUpdate = DateTime.UtcNow;
        }
    }
}
