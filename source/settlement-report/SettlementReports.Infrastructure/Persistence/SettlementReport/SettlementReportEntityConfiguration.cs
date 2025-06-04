using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence.SettlementReport;

public class SettlementReportEntityConfiguration : IEntityTypeConfiguration<Application.SettlementReports.SettlementReport>
{
    public void Configure(EntityTypeBuilder<Application.SettlementReports.SettlementReport> builder)
    {
        builder.ToTable("SettlementReport");
        builder.HasKey(e => e.Id);
    }
}
