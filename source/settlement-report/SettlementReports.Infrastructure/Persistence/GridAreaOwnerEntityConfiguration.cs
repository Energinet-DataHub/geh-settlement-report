using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence;

public class GridAreaOwnerEntityConfiguration : IEntityTypeConfiguration<GridAreaOwnerEntity>
{
    public void Configure(EntityTypeBuilder<GridAreaOwnerEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("GridAreaOwner");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Code).HasMaxLength(4);
        builder.Property(e => e.ActorNumber).HasMaxLength(50);
    }
}
