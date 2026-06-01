namespace Movie_StructureCode.Domain.Entities
{
    public class Room : BaseEntities
    {
        public string Name { get; private set; }
        public int TotalSeat { get; private set; }
        public int TotalRow { get; private set; }
        public int TotalCol { get; private set; }

        // foreign key
        public Guid TheaterId { get; private set; }
        public Theater? Theater { get; private set; }

        public ICollection<Seat>? Seats { get; private set; }
        public ICollection<Showing>? Showings { get; private set; }

        private Room() : base() { }

        private Room(Guid id, string name, int totalSeat, int totalRow, int totalCol, Guid theaterId, bool isActive, DateTime dateCreate, DateTime dateUpdate)
            : base(id, dateCreate, dateUpdate, isActive)
        {
            Name = name;
            TotalSeat = totalSeat;
            TotalRow = totalRow;
            TotalCol = totalCol;
            TheaterId = theaterId;
        }

        public static Room Create(Guid id, string name, int totalSeat, int totalRow, int totalCol, Guid theaterId, bool isActive = true)
        {
            return new Room(
                id,
                name,
                totalSeat,
                totalRow,
                totalCol,
                theaterId,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public void UpdateDetails(string name, int totalSeat, int totalRow, int totalCol, Guid theaterId, bool isActive)
        {
            Name = name;
            TotalSeat = totalSeat;
            TotalRow = totalRow;
            TotalCol = totalCol;
            TheaterId = theaterId;
            IsActive = isActive;
            DateUpdate = DateTime.UtcNow;
        }
    }
}
