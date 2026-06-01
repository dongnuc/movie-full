namespace Movie_StructureCode.Domain.Entities
{
    public class SeatType : BaseEntities
    {
        public string Name { get; private set; } = string.Empty;
        public decimal Surcharge { get; private set; }

        public ICollection<Seat>? Seats { get; private set; }

        private SeatType() : base() { }

        private SeatType(Guid id, string name, decimal surcharge, bool isActive, DateTime dateCreate, DateTime dateUpdate)
            : base(id, dateCreate, dateUpdate, isActive)
        {
            Name = name;
            Surcharge = surcharge;
        }

        public static SeatType Create(Guid id, string name, decimal surcharge, bool isActive = true)
        {
            return new SeatType(
                id,
                name,
                surcharge,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public void UpdateDetails(string name, decimal surcharge, bool isActive)
        {
            Name = name;
            Surcharge = surcharge;
            IsActive = isActive;
            DateUpdate = DateTime.UtcNow;
        }
    }
}
