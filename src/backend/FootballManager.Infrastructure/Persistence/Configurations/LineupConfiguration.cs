using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Infrastructure.Persistence.Configurations;

public sealed class LineupConfiguration : IEntityTypeConfiguration<Lineup>
{
    public void Configure(EntityTypeBuilder<Lineup> builder)
    {
        builder.ToTable("lineups");
        builder.HasKey(lineup => lineup.Id);

        builder.Property(lineup => lineup.Id)
            .ValueGeneratedNever();

        builder.Property(lineup => lineup.GameSaveId)
            .HasColumnName("game_save_id")
            .IsRequired();

        builder.Property(lineup => lineup.FormationId)
            .HasColumnName("formation_id")
            .IsRequired();

        builder.Property(lineup => lineup.StarterPlayerIds)
            .HasColumnName("starter_player_ids")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(lineup => lineup.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(lineup => lineup.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(lineup => lineup.GameSaveId)
            .IsUnique();

        builder.HasOne(lineup => lineup.GameSave)
            .WithOne(gameSave => gameSave.Lineup)
            .HasForeignKey<Lineup>(lineup => lineup.GameSaveId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lineup => lineup.Formation)
            .WithMany(formation => formation.Lineups)
            .HasForeignKey(lineup => lineup.FormationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
