namespace Movie_StructureCode.Domain.Entities
{
    public class Seat : BaseEntities
    {
        public int RowIndex { get; private set; } // do cot va hang cua room quyet dinh
        public int ColIndex { get; private set; }
        public string RowName { get; private set; } = string.Empty;
        public int SeatNumber { get; private set; }

        public Guid? GroupId { get; private set; }

        // foreign key

        public Guid SeatTypeId { get; private set; }
        public SeatType SeatType { get; private set; }

        public Guid RoomId { get; private set; }
        public Room? Room { get; private set; }

        private Seat() : base() { }

        private Seat(
            Guid id,
            int rowIndex,
            int colIndex,
            string rowName,
            int seatNumber,
            Guid? groupId,
            Guid seatTypeId,
            Guid roomId,
            bool isActive,
            DateTime dateCreate,
            DateTime dateUpdate)
            : base(id, dateCreate, dateUpdate, isActive)
        {
            RowIndex = rowIndex;
            ColIndex = colIndex;
            RowName = rowName;
            SeatNumber = seatNumber;
            GroupId = groupId;
            SeatTypeId = seatTypeId;
            RoomId = roomId;
        }

        public static Seat Create(
            Guid id,
            int rowIndex,
            int colIndex,
            string rowName,
            int seatNumber,
            Guid? groupId,
            Guid seatTypeId,
            Guid roomId,
            bool isActive = true)
        {
            return new Seat(
                id,
                rowIndex,
                colIndex,
                rowName,
                seatNumber,
                groupId,
                seatTypeId,
                roomId,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public void UpdateDetails(
            int rowIndex,
            int colIndex,
            string rowName,
            int seatNumber,
            Guid? groupId,
            Guid seatTypeId,
            Guid roomId,
            bool isActive)
        {
            RowIndex = rowIndex;
            ColIndex = colIndex;
            RowName = rowName;
            SeatNumber = seatNumber;
            GroupId = groupId;
            SeatTypeId = seatTypeId;
            RoomId = roomId;
            IsActive = isActive;
            DateUpdate = DateTime.UtcNow;
        }



        public void UpdateSeatNumber(int seatNumber)
        {
            SeatNumber = seatNumber;
            DateUpdate = DateTime.UtcNow;
        }

        public void UpdateSeatType(Guid seatTypeId)
        {
            SeatTypeId = seatTypeId;
            DateUpdate = DateTime.UtcNow;
        }

        public void UpdateGroupId(Guid? groupId)
        {
            GroupId = groupId;
            DateUpdate = DateTime.UtcNow;
        }

    }
}
