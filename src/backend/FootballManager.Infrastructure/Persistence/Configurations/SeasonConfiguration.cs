using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Infrastructure.Persistence.Configurations;

public sealed class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> builder)
    {
        builder.ToTable("seasons");
        builder.HasKey(season => season.Id);

        builder.Property(season => season.Id)
            .ValueGeneratedNever();

        builder.Property(season => season.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(season => season.StartsAt)
            .HasColumnName("starts_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(season => season.CurrentRound)
            .HasColumnName("current_round")
            .IsRequired();

        builder.Property(season => season.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(season => season.LeagueId)
            .HasColumnName("league_id")
            .IsRequired();

        builder.HasIndex(season => new { season.LeagueId, season.Name })
            .IsUnique();

        builder.HasMany(season => season.Fixtures)
            .WithOne(fixture => fixture.Season)
            .HasForeignKey(fixture => fixture.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
