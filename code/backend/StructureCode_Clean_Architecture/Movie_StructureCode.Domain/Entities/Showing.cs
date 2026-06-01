namespace Movie_StructureCode.Domain.Entities
{
    public class Showing : BaseEntities
    {
        public DateTime TimeStart { get; private set; }
        public decimal Price { get; private set; }
        public int MaxTicketsPerUser { get; private set; }

        //foreign key

        public Guid MovieId { get; private set; }
        public Movie? Movie { get; private set; }

        public Guid RoomId { get; private set; }
        public Room? Room { get; private set; }

        /// <summary>
        /// Denormalized field: Theater ID (từ Room.TheaterId)
        /// Được set tự động khi tạo Showing
        /// Dùng để tối ưu query khi lấy movies theo theater
        /// </summary>
        public Guid TheaterId { get; private set; }

        private Showing() : base() { }

        private Showing(
            Guid id,
            DateTime timeStart,
            decimal price,
            int maxTicketsPerUser,
            Guid movieId,
            Guid roomId,
            Guid theaterId,
            bool isActive,
            DateTime dateCreate,
            DateTime dateUpdate)
            : base(id, dateCreate, dateUpdate, isActive)
        {
            TimeStart = timeStart;
            Price = price;
            MaxTicketsPerUser = maxTicketsPerUser;
            MovieId = movieId;
            RoomId = roomId;
            TheaterId = theaterId;
        }

        public static Showing Create(
            Guid id,
            DateTime timeStart,
            decimal price,
            int maxTicketsPerUser,
            Guid movieId,
            Guid roomId,
            Guid theaterId,
            bool isActive = true)
        {
            return new Showing(
                id,
                timeStart,
                price,
                maxTicketsPerUser,
                movieId,
                roomId,
                theaterId,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public void UpdateDetails(
            DateTime timeStart,
            decimal price,
            int maxTicketsPerUser,
            Guid roomId,
            Guid theaterId,
            bool isActive)
        {
            TimeStart = timeStart;
            Price = price;
            MaxTicketsPerUser = maxTicketsPerUser;
            RoomId = roomId;
            TheaterId = theaterId;
            IsActive = isActive;
            DateUpdate = DateTime.UtcNow;
        }
    }
}
