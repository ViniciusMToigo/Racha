using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Racha.Domain.Expenses;
using Racha.Domain.Groups;
using Racha.Domain.Users;

namespace Racha.Infrastructure.Persistence.Configurations;

internal sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Description).HasMaxLength(Expense.MaxDescriptionLength).IsRequired();

        // numeric(12,2): exact money arithmetic, up to 9,999,999,999.99.
        builder.Property(e => e.Amount).HasPrecision(12, 2);

        builder.Property(e => e.Date);

        // Deleting a group deletes its expenses.
        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(e => e.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.PaidByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Deleting an expense deletes its shares.
        builder.HasMany(e => e.Shares)
            .WithOne()
            .HasForeignKey(s => s.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Shares)
            .HasField("_shares")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Listing a group's expenses by date is the most common query.
        builder.HasIndex(e => new { e.GroupId, e.Date });

        builder.Property<DateTimeOffset>(RachaDbContext.CreatedAtColumn);
    }
}
