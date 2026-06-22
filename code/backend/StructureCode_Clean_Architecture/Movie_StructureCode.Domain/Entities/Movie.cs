namespace Movie_StructureCode.Domain.Entities
{
    public class Movie : BaseEntities
    {
        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string? Image { get; private set; }
        public string? ImageBanner { get; private set; }
        public string? ImagePoster { get; private set; }
        public bool IsHomePage { get; private set; }
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
            string? imageBanner,
            string? imagePoster,
            bool isHomePage,
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
            ImageBanner = imageBanner;
            ImagePoster = imagePoster;
            IsHomePage = isHomePage;
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
            bool isActive = true,
            string? imageBanner = null,
            string? imagePoster = null,
            bool isHomePage = false)
        {
            return new Movie(
                id: id,
                title: title,
                description: description,
                image: image,
                imageBanner: imageBanner,
                imagePoster: imagePoster,
                isHomePage: isHomePage,
                basePrice: basePrice,
                duration: duration,
                categoryId: categoryId,
                isActive: isActive,
                dateCreate: DateTime.UtcNow,
                dateUpdate: DateTime.UtcNow);
        }

        public void UpdateDetails(
            string title,
            string? description,
            string? image,
            decimal basePrice,
            int duration,
            Guid categoryId,
            bool isActive,
            string? imageBanner = null,
            string? imagePoster = null,
            bool isHomePage = false)
        {
            Title = title;
            Description = description;
            Image = image;
            if (imageBanner != null) ImageBanner = imageBanner;
            if (imagePoster != null) ImagePoster = imagePoster;
            IsHomePage = isHomePage;
            BasePrice = basePrice;
            Duration = duration;
            CategoryId = categoryId;
            IsActive = isActive;
            DateUpdate = DateTime.UtcNow;
        }

        public void UpdateBanner(string? imageBanner)
        {
            ImageBanner = imageBanner;
            DateUpdate = DateTime.UtcNow;
        }

        public void UpdatePoster(string? imagePoster)
        {
            ImagePoster = imagePoster;
            DateUpdate = DateTime.UtcNow;
        }

        public void UpdateHomePageStatus(bool isHomePage)
        {
            IsHomePage = isHomePage;
            DateUpdate = DateTime.UtcNow;
        }
    }
}
