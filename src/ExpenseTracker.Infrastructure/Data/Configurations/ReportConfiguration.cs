using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        builder.Property(r => r.ReportType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Parameters)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

        builder.Property(r => r.PublicId)
            .IsRequired();

        builder.HasIndex(r => r.PublicId)
            .IsUnique();

        builder.Property(r => r.FileName)
            .HasMaxLength(256);

        builder.Property(r => r.ContentType)
            .HasMaxLength(128);

        builder.Property(r => r.Content)
            .HasColumnType("varbinary(max)");

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reports)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

