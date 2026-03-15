using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Infrastructure.Persistence.Configurations;

public sealed class FormationConfiguration : IEntityTypeConfiguration<Formation>
{
    public void Configure(EntityTypeBuilder<Formation> builder)
    {
        builder.ToTable("formations");
        builder.HasKey(formation => formation.Id);

        builder.Property(formation => formation.Id)
            .ValueGeneratedNever();

        builder.Property(formation => formation.Name)
            .HasColumnName("name")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(formation => formation.Defenders)
            .HasColumnName("defenders")
            .IsRequired();

        builder.Property(formation => formation.Midfielders)
            .HasColumnName("midfielders")
            .IsRequired();

        builder.Property(formation => formation.Forwards)
            .HasColumnName("forwards")
            .IsRequired();

        builder.Property(formation => formation.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(formation => formation.Name)
            .IsUnique();
    }
}
