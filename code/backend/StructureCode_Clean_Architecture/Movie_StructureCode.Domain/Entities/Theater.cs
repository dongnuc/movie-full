namespace Movie_StructureCode.Domain.Entities
{
    public class Theater : BaseEntities
    {
        public string Name { get; private set; } = string.Empty;
        public string? Location { get; private set; }

        public ICollection<Room>? Rooms { get; private set; }
        public ICollection<Theater_User>? TheaterUsers { get; private set; }

        // Parameterless constructor for EF Core
        private Theater() : base() { }

        private Theater(Guid id, string name, string? location, bool isActive,
             DateTime dateCreate, DateTime dateUpdate)
            : base(id, dateCreate, dateUpdate, isActive)
        {
            Name = name;
            Location = location;
        }

        public static Theater Create(Guid id, string name, string? location, bool isActive = true)
        {
            return new Theater(
                id,
                name,
                location,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public void UpdateDetails(string name, string? location, bool isActive)
        {
            Name = name;
            Location = location;
            IsActive = isActive;
            DateUpdate = DateTime.UtcNow;
        }
    }
}
