using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Data.Configurations;

public class RecurringExpenseConfiguration : IEntityTypeConfiguration<RecurringExpense>
{
    public void Configure(EntityTypeBuilder<RecurringExpense> builder)
    {
        builder.ToTable("RecurringExpenses");

        builder.Property(r => r.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(r => r.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(512);

        builder.Property(r => r.PublicId)
            .IsRequired();

        builder.HasIndex(r => r.PublicId)
            .IsUnique();

        builder.Property(r => r.Frequency)
            .HasConversion(
                value => value.ToString(),
                value => Enum.Parse<RecurrenceFrequency>(value))
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(r => r.StartDate)
            .HasConversion(
                value => value.ToDateTime(TimeOnly.MinValue),
                value => DateOnly.FromDateTime(value))
            .HasColumnType("date");

        builder.Property(r => r.EndDate)
            .HasConversion(
                value => value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                value => value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null)
            .HasColumnType("date");

        builder.Property(r => r.NextOccurrence)
            .HasConversion(
                value => value.ToDateTime(TimeOnly.MinValue),
                value => DateOnly.FromDateTime(value))
            .HasColumnType("date");

        builder.HasOne(r => r.Category)
            .WithMany(c => c.RecurringExpenses)
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => new { r.UserId, r.IsActive, r.NextOccurrence });
    }
}

