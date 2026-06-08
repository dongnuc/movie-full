namespace Movie_StructureCode.Domain.Entities
{
    public class Showing : BaseEntities
    {
        public DateTime TimeStart { get; private set; }

        /// <summary>
        /// Thời điểm kết thúc hoàn toàn của showing
        /// Bao gồm movie duration + buffer time
        /// </summary>
        public DateTime EndTime { get; private set; }

        /// <summary>
        /// Thời gian dọn phòng / trailer / ads
        /// </summary>
        public int BufferTime { get; private set; } = 10;

        /// <summary>
        /// Snapshot runtime của movie tại thời điểm tạo showing
        /// </summary>
        public int TimeDuration { get; private set; }

        public decimal Price { get; private set; }

        public int MaxTicketsPerUser { get; private set; }

        // Foreign Key

        public Guid MovieId { get; private set; }
        public Movie? Movie { get; private set; }

        public Guid RoomId { get; private set; }
        public Room? Room { get; private set; }

        /// <summary>
        /// Denormalized field: Theater ID (từ Room.TheaterId)
        /// Dùng để tối ưu query
        /// </summary>
        public Guid TheaterId { get; private set; }

        private Showing() : base()
        {
        }

        private Showing(
            Guid id,
            DateTime timeStart,
            DateTime endTime,
            decimal price,
            int maxTicketsPerUser,
            Guid movieId,
            Guid roomId,
            Guid theaterId,
            int timeDuration,
            int bufferTime,
            bool isActive,
            DateTime dateCreate,
            DateTime dateUpdate)
            : base(id, dateCreate, dateUpdate, isActive)
        {
            TimeStart = timeStart;
            EndTime = endTime;

            Price = price;
            MaxTicketsPerUser = maxTicketsPerUser;

            MovieId = movieId;
            RoomId = roomId;
            TheaterId = theaterId;

            TimeDuration = timeDuration;
            BufferTime = bufferTime;
        }

        public static Showing Create(
            Guid id,
            DateTime timeStart,
            decimal price,
            int maxTicketsPerUser,
            Guid movieId,
            Guid roomId,
            Guid theaterId,
            int timeDuration,
            int bufferTime = 10,
            bool isActive = true)
        {
            if (timeDuration <= 0)
            {
                throw new ArgumentException("Movie duration must be greater than 0.");
            }

            if (bufferTime < 0)
            {
                throw new ArgumentException("Buffer time cannot be negative.");
            }

            DateTime endTime =
                timeStart.AddMinutes(timeDuration + bufferTime);

            return new Showing(
                id,
                timeStart,
                endTime,
                price,
                maxTicketsPerUser,
                movieId,
                roomId,
                theaterId,
                timeDuration,
                bufferTime,
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

            EndTime =
                timeStart.AddMinutes(TimeDuration + BufferTime);

            Price = price;
            MaxTicketsPerUser = maxTicketsPerUser;

            RoomId = roomId;
            TheaterId = theaterId;

            IsActive = isActive;

            DateUpdate = DateTime.UtcNow;
        }
    }
}
