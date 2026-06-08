using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.Configurations
{
    public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Jti)
                .HasMaxLength(36)
                .IsRequired();
            builder.HasIndex(x => x.Jti)
                .IsUnique();

            builder.Property(x => x.AccessJti)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.UsedAt)
                .IsRequired(false);

            builder.Property(x => x.ExpiredAt)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.DeviceName)
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(x => x.DeviceType)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(x => x.Ip)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(x => x.LoginAt)
                .IsRequired(false);

            builder.HasOne(x => x.AppUser)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
