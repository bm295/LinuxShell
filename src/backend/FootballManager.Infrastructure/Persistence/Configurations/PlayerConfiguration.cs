using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Infrastructure.Persistence.Configurations;

public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("players");
        builder.HasKey(player => player.Id);

        builder.Property(player => player.Id)
            .ValueGeneratedNever();

        builder.Property(player => player.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(player => player.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(player => player.Position)
            .HasColumnName("position")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(player => player.SquadNumber)
            .HasColumnName("squad_number")
            .IsRequired();

        builder.Property(player => player.Attack)
            .HasColumnName("attack")
            .IsRequired();

        builder.Property(player => player.Defense)
            .HasColumnName("defense")
            .IsRequired();

        builder.Property(player => player.Passing)
            .HasColumnName("passing")
            .IsRequired();

        builder.Property(player => player.Fitness)
            .HasColumnName("fitness")
            .IsRequired();

        builder.Property(player => player.Morale)
            .HasColumnName("morale")
            .IsRequired();

        builder.Property(player => player.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(player => player.ClubId)
            .HasColumnName("club_id")
            .IsRequired();

        builder.HasIndex(player => new { player.ClubId, player.SquadNumber })
            .IsUnique();
    }
}
