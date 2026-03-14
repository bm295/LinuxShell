using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Infrastructure.Persistence.Configurations;

public sealed class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.ToTable("leagues");
        builder.HasKey(league => league.Id);

        builder.Property(league => league.Id)
            .ValueGeneratedNever();

        builder.Property(league => league.Name)
            .HasMaxLength(120)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(league => league.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasMany(league => league.Clubs)
            .WithOne(club => club.League)
            .HasForeignKey(club => club.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
