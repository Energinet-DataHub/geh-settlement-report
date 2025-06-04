using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;

public class MeasurementsReportEntityConfiguration : IEntityTypeConfiguration<Application.MeasurementsReport.MeasurementsReport>
{
    public void Configure(EntityTypeBuilder<Application.MeasurementsReport.MeasurementsReport> builder)
    {
        builder.ToTable("MeasurementsReport");
        builder.HasKey(e => e.Id);
    }
}
