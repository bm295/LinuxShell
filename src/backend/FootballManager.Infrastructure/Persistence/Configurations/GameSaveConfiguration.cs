using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Infrastructure.Persistence.Configurations;

public sealed class GameSaveConfiguration : IEntityTypeConfiguration<GameSave>
{
    public void Configure(EntityTypeBuilder<GameSave> builder)
    {
        builder.ToTable("game_saves");
        builder.HasKey(gameSave => gameSave.Id);

        builder.Property(gameSave => gameSave.Id)
            .ValueGeneratedNever();

        builder.Property(gameSave => gameSave.SelectedClubId)
            .HasColumnName("selected_club_id")
            .IsRequired();

        builder.Property(gameSave => gameSave.SeasonId)
            .HasColumnName("season_id")
            .IsRequired();

        builder.Property(gameSave => gameSave.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(gameSave => gameSave.SeasonId)
            .IsUnique();

        builder.HasOne(gameSave => gameSave.SelectedClub)
            .WithMany()
            .HasForeignKey(gameSave => gameSave.SelectedClubId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(gameSave => gameSave.Season)
            .WithMany()
            .HasForeignKey(gameSave => gameSave.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
