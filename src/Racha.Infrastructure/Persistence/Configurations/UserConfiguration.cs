using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Racha.Domain.Users;

namespace Racha.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Name).HasMaxLength(User.MaxNameLength).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(User.MaxEmailLength).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();

        // The real guarantee that e-mails are unique (the application check alone can race).
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property<DateTimeOffset>(RachaDbContext.CreatedAtColumn);
    }
}
