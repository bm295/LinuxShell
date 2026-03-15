using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Infrastructure.Persistence.Configurations;

public sealed class ClubConfiguration : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> builder)
    {
        builder.ToTable("clubs");
        builder.HasKey(club => club.Id);

        builder.Property(club => club.Id)
            .ValueGeneratedNever();

        builder.Property(club => club.Name)
            .HasMaxLength(120)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(club => club.TransferBudget)
            .HasColumnName("transfer_budget")
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(club => club.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(club => club.LeagueId)
            .HasColumnName("league_id")
            .IsRequired();

        builder.HasIndex(club => new { club.LeagueId, club.Name })
            .IsUnique();

        builder.HasMany(club => club.Players)
            .WithOne(player => player.Club)
            .HasForeignKey(player => player.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(club => club.AcademyPlayers)
            .WithOne(player => player.Club)
            .HasForeignKey(player => player.ClubId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
