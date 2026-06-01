namespace Movie_StructureCode.Domain.Entities
{
    public class BaseEntities 
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime DateCreate { get; set; }
        public DateTime DateUpdate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = false;

        protected BaseEntities() { }

        protected BaseEntities(Guid id, DateTime dateCreate, DateTime dateUpdate, bool isActive)
        {
            Id = id;
            DateCreate = dateCreate;
            DateUpdate = dateUpdate;
            IsActive = isActive;
        }

        public void SetIsActive(bool isActive)
        {
            IsActive = isActive;
            DateUpdate = DateTime.UtcNow;
        }
    }
}
