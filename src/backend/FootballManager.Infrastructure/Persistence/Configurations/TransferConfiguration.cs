using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Infrastructure.Persistence.Configurations;

public sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("transfers");
        builder.HasKey(transfer => transfer.Id);

        builder.Property(transfer => transfer.Id)
            .ValueGeneratedNever();

        builder.Property(transfer => transfer.PlayerId)
            .HasColumnName("player_id")
            .IsRequired();

        builder.Property(transfer => transfer.FromClubId)
            .HasColumnName("from_club_id")
            .IsRequired();

        builder.Property(transfer => transfer.ToClubId)
            .HasColumnName("to_club_id")
            .IsRequired();

        builder.Property(transfer => transfer.Fee)
            .HasColumnName("fee")
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(transfer => transfer.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne(transfer => transfer.Player)
            .WithMany()
            .HasForeignKey(transfer => transfer.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(transfer => transfer.FromClub)
            .WithMany()
            .HasForeignKey(transfer => transfer.FromClubId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(transfer => transfer.ToClub)
            .WithMany()
            .HasForeignKey(transfer => transfer.ToClubId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
