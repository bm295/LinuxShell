using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Infrastructure.Persistence.Configurations;

public sealed class FinanceEntryConfiguration : IEntityTypeConfiguration<FinanceEntry>
{
    public void Configure(EntityTypeBuilder<FinanceEntry> builder)
    {
        builder.ToTable("finance_entries");
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .ValueGeneratedNever();

        builder.Property(entry => entry.ClubId)
            .HasColumnName("club_id")
            .IsRequired();

        builder.Property(entry => entry.Type)
            .HasColumnName("entry_type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(entry => entry.Amount)
            .HasColumnName("amount")
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(entry => entry.Description)
            .HasColumnName("description")
            .HasMaxLength(240)
            .IsRequired();

        builder.Property(entry => entry.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne(entry => entry.Club)
            .WithMany()
            .HasForeignKey(entry => entry.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(entry => new { entry.ClubId, entry.OccurredAt });
    }
}
