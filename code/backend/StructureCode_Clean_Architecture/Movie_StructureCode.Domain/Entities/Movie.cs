namespace Movie_StructureCode.Domain.Entities
{
    public class Movie : BaseEntities
    {
        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string? Image { get; private set; }
        public decimal BasePrice { get; private set; }
        public int Duration { get; private set; }

        public Guid CategoryId { get; private set; }
        public Category? Category { get; private set; }

        public ICollection<Showing>? Showings { get; private set; }

        // Parameterless constructor for EF Core
        private Movie() : base() { }

        private Movie(
            Guid id,
            string title,
            string? description,
            string? image,
            decimal basePrice,
            int duration,
            Guid categoryId,
            bool isActive,
            DateTime dateCreate,
            DateTime dateUpdate)
            : base(id, dateCreate, dateUpdate, isActive)
        {
            Title = title;
            Description = description;
            Image = image;
            BasePrice = basePrice;
            Duration = duration;
            CategoryId = categoryId;
        }

        public static Movie Create(
            Guid id,
            string title,
            string? description,
            string? image,
            decimal basePrice,
            int duration,
            Guid categoryId,
            bool isActive = true)
        {
            return new Movie(
                id,
                title,
                description,
                image,
                basePrice,
                duration,
                categoryId,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public void UpdateDetails(
            string title,
            string? description,
            string? image,
            decimal basePrice,
            int duration,
            Guid categoryId,
            bool isActive)
        {
            Title = title;
            Description = description;
            Image = image;
            BasePrice = basePrice;
            Duration = duration;
            CategoryId = categoryId;
            IsActive = isActive;
            DateUpdate = DateTime.UtcNow;
        }
    }
}
