using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Racha.Domain.Expenses;
using Racha.Domain.Users;

namespace Racha.Infrastructure.Persistence.Configurations;

internal sealed class ExpenseShareConfiguration : IEntityTypeConfiguration<ExpenseShare>
{
    public void Configure(EntityTypeBuilder<ExpenseShare> builder)
    {
        builder.ToTable("expense_shares");

        builder.HasKey(s => new { s.ExpenseId, s.UserId });

        builder.Property(s => s.Amount).HasPrecision(12, 2);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
