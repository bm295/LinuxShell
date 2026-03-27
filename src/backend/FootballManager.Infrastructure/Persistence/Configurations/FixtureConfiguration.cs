using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Infrastructure.Persistence.Configurations;

public sealed class FixtureConfiguration : IEntityTypeConfiguration<Fixture>
{
    public void Configure(EntityTypeBuilder<Fixture> builder)
    {
        builder.ToTable("fixtures");
        builder.HasKey(fixture => fixture.Id);

        builder.Property(fixture => fixture.Id)
            .ValueGeneratedNever();

        builder.Property(fixture => fixture.SeasonId)
            .HasColumnName("season_id")
            .IsRequired();

        builder.Property(fixture => fixture.HomeClubId)
            .HasColumnName("home_club_id")
            .IsRequired();

        builder.Property(fixture => fixture.AwayClubId)
            .HasColumnName("away_club_id")
            .IsRequired();

        builder.Property(fixture => fixture.RoundNumber)
            .HasColumnName("round_number")
            .IsRequired();

        builder.Property(fixture => fixture.ScheduledAt)
            .HasColumnName("scheduled_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(fixture => fixture.IsPlayed)
            .HasColumnName("is_played")
            .IsRequired();

        builder.Property(fixture => fixture.HomeGoals)
            .HasColumnName("home_goals");

        builder.Property(fixture => fixture.AwayGoals)
            .HasColumnName("away_goals");

        builder.Property(fixture => fixture.MatchMvpPlayerId)
            .HasColumnName("match_mvp_player_id");

        builder.Property(fixture => fixture.PlayedAt)
            .HasColumnName("played_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(fixture => fixture.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(fixture => new { fixture.SeasonId, fixture.RoundNumber, fixture.HomeClubId, fixture.AwayClubId })
            .IsUnique();

        builder.HasOne(fixture => fixture.HomeClub)
            .WithMany()
            .HasForeignKey(fixture => fixture.HomeClubId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fixture => fixture.AwayClub)
            .WithMany()
            .HasForeignKey(fixture => fixture.AwayClubId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fixture => fixture.MatchMvpPlayer)
            .WithMany()
            .HasForeignKey(fixture => fixture.MatchMvpPlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
