using Energinet.DataHub.Reports.Application.SettlementReports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence.SettlementReportRequest;

public class SettlementReportEntityConfiguration : IEntityTypeConfiguration<SettlementReport>
{
    public void Configure(EntityTypeBuilder<SettlementReport> builder)
    {
        builder.ToTable("SettlementReport");
        builder.HasKey(e => e.Id);
    }
}
