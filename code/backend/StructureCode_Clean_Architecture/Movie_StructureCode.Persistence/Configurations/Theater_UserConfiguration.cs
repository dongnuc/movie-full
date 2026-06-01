using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.Configurations
{
    public sealed class Theater_UserConfiguration : IEntityTypeConfiguration<Theater_User>
    {
        public void Configure(EntityTypeBuilder<Theater_User> builder)
        {
            builder.ToTable(nameof(Theater_User));

            builder.HasKey(x => x.Id);

            // Configure relationship to Theater (Cascade delete)
            builder.HasOne(x => x.Theater)
                .WithMany(t => t.TheaterUsers)
                .HasForeignKey(x => x.TheaterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship to AppUser (Restrict delete to prevent circular cascade paths)
            builder.HasOne(x => x.User)
                .WithMany(u => u.TheaterUsers)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure IsPrimary column
            builder.Property(x => x.IsPrimary)
                .IsRequired();

            // Ensure unique relationship per Theater and User
            builder.HasIndex(x => new { x.TheaterId, x.UserId })
                .IsUnique();
        }
    }
}
