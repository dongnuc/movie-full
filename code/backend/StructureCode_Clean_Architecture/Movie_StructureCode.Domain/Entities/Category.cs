namespace Movie_StructureCode.Domain.Entities
{
    public class Category : BaseEntities
    {
        public string Name { get; private set; } = string.Empty;

        public ICollection<Movie>? Movies { get; private set; }

        private Category() : base() { }

        private Category(Guid id, string name, bool isActive, DateTime dateCreate, DateTime dateUpdate)
            : base(id, dateCreate, dateUpdate, isActive)
        {
            Name = name;
        }

        public static Category Create(Guid id, string name, bool isActive = true)
        {
            return new Category(
                id,
                name,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public void UpdateDetails(string name, bool isActive)
        {
            Name = name;
            IsActive = isActive;
            DateUpdate = DateTime.UtcNow;
        }
    }
}
