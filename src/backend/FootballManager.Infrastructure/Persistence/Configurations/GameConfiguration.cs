using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Infrastructure.Persistence.Configurations;

public sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("games");
        builder.HasKey(game => game.Id);

        builder.Property(game => game.Id)
            .ValueGeneratedNever();

        builder.Property(game => game.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
