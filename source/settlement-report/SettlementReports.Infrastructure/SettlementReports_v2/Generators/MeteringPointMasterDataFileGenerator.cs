// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using CsvHelper;
using CsvHelper.Configuration;
using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.CalculationResults.Model;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;

namespace Energinet.DataHub.SettlementReport.Infrastructure.SettlementReports_v2.Generators;

public sealed class MeteringPointMasterDataFileGenerator : CsvFileGeneratorBase<SettlementReportMeteringPointMasterDataRow, MeteringPointMasterDataFileGenerator.SettlementReportMeteringPointMasterDataRowMap>
{
    private readonly ISettlementReportMeteringPointMasterDataRepository _dataSource;

    public MeteringPointMasterDataFileGenerator(ISettlementReportMeteringPointMasterDataRepository dataSource)
        : base(
            int.MaxValue, //200_000, // 5 rows in each chunk, 1.000.000 rows per chunk in total.
            quotedColumns: [0, 3, 4, 5, 8])
    {
        _dataSource = dataSource;
    }

    protected override Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion)
    {
        return _dataSource.CountAsync(filter, actorInfo, maximumCalculationVersion);
    }

    protected override IAsyncEnumerable<SettlementReportMeteringPointMasterDataRow> GetAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion, int skipChunks, int takeChunks)
    {
        return _dataSource.GetAsync(filter, actorInfo, skipChunks, takeChunks, maximumCalculationVersion);
    }

    protected override void RegisterClassMap(CsvWriter csvHelper, SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo)
    {
        csvHelper.Context.RegisterClassMap(new SettlementReportMeteringPointMasterDataRowMap(actorInfo));
    }

    public sealed class SettlementReportMeteringPointMasterDataRowMap : ClassMap<SettlementReportMeteringPointMasterDataRow>
    {
        public SettlementReportMeteringPointMasterDataRowMap(SettlementReportRequestedByActor actorInfo)
        {
            var columnIndex = 0;
            Map(r => r.MeteringPointId)
                .Name("METERINGPOINTID")
                .Index(columnIndex++);

            Map(r => r.PeriodStart)
                .Name("VALIDFROM")
                .Index(columnIndex++);

            Map(r => r.PeriodEnd)
                .Name("VALIDTO")
                .Index(columnIndex++);

            Map(r => r.GridAreaId)
                .Name("GRIDAREAID")
                .Index(columnIndex++)
                .Convert(row => row.Value.GridAreaId.PadLeft(3, '0'));

            if (actorInfo.MarketRole is not MarketRole.EnergySupplier)
            {
                Map(r => r.GridAreaToId)
                    .Name("TOGRIDAREAID")
                    .Index(columnIndex++)
                    .Convert(row => row.Value.GridAreaToId?.PadLeft(3, '0'));

                Map(r => r.GridAreaFromId)
                    .Name("FROMGRIDAREAID")
                    .Index(columnIndex++)
                    .Convert(row => row.Value.GridAreaFromId?.PadLeft(3, '0'));
            }

            Map(r => r.MeteringPointType)
                .Name("TYPEOFMP")
                .Index(columnIndex++)
                .Convert(row => row.Value.MeteringPointType switch
                {
                    null => string.Empty,
                    MeteringPointType.Consumption => "E17",
                    MeteringPointType.Production => "E18",
                    MeteringPointType.Exchange => "E20",
                    MeteringPointType.VeProduction => "D01",
                    MeteringPointType.NetProduction => "D05",
                    MeteringPointType.SupplyToGrid => "D06",
                    MeteringPointType.ConsumptionFromGrid => "D07",
                    MeteringPointType.WholesaleServicesInformation => "D08",
                    MeteringPointType.OwnProduction => "D09",
                    MeteringPointType.NetFromGrid => "D10",
                    MeteringPointType.NetToGrid => "D11",
                    MeteringPointType.TotalConsumption => "D12",
                    MeteringPointType.ElectricalHeating => "D14",
                    MeteringPointType.NetConsumption => "D15",
                    MeteringPointType.EffectSettlement => "D19",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.MeteringPointType)),
                });

            Map(r => r.SettlementMethod)
                .Name("SETTLEMENTMETHOD")
                .Index(columnIndex++)
                .Convert(row => row.Value.SettlementMethod switch
                {
                    null => string.Empty,
                    SettlementMethod.NonProfiled => "E02",
                    SettlementMethod.Flex => "D01",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.SettlementMethod)),
                });

            if (actorInfo.MarketRole is MarketRole.DataHubAdministrator)
            {
                Map(r => r.EnergySupplierId)
                    .Name("ENERGYSUPPLIERID")
                    .Index(columnIndex++);
            }
        }
    }
}
