﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.SettlementReport.Interfaces.CalculationResults.Model.WholesaleResults;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;
using static Energinet.DataHub.SettlementReport.Infrastructure.SettlementReports_v2.Generators.ChargeLinkPeriodsFileGenerator;

namespace Energinet.DataHub.SettlementReport.Infrastructure.SettlementReports_v2.Generators;

public sealed class ChargeLinkPeriodsFileGenerator : CsvFileGeneratorBase<SettlementReportChargeLinkPeriodsResultRow, SettlementReportChargeLinkPeriodsResultRowMap>
{
    private readonly ISettlementReportChargeLinkPeriodsRepository _dataSource;

    public ChargeLinkPeriodsFileGenerator(ISettlementReportChargeLinkPeriodsRepository dataSource)
        : base(
            int.MaxValue, //40_000, // About 25 rows per metering point, about 1.000.000 rows per chunk in total.
            quotedColumns: [0, 3, 8])
    {
        _dataSource = dataSource;
    }

    protected override Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion)
    {
        return _dataSource.CountAsync(filter, actorInfo);
    }

    protected override IAsyncEnumerable<SettlementReportChargeLinkPeriodsResultRow> GetAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion, int skipChunks, int takeChunks)
    {
        return _dataSource.GetAsync(filter, actorInfo, skipChunks, takeChunks);
    }

    protected override void RegisterClassMap(CsvWriter csvHelper, SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo)
    {
        csvHelper.Context.RegisterClassMap(new SettlementReportChargeLinkPeriodsResultRowMap(actorInfo));
    }

    public sealed class SettlementReportChargeLinkPeriodsResultRowMap : ClassMap<SettlementReportChargeLinkPeriodsResultRow>
    {
        public SettlementReportChargeLinkPeriodsResultRowMap(SettlementReportRequestedByActor actorInfo)
        {
            Map(r => r.MeteringPointId)
                .Name("METERINGPOINTID")
                .Index(0);

            Map(r => r.MeteringPointType)
                .Name("TYPEOFMP")
                .Index(1)
                .Convert(row => row.Value.MeteringPointType switch
                {
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

            Map(r => r.ChargeType)
                .Name("CHARGETYPE")
                .Index(2)
                .Convert(row => row.Value.ChargeType switch
                {
                    ChargeType.Tariff => "D03",
                    ChargeType.Fee => "D02",
                    ChargeType.Subscription => "D01",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.ChargeType)),
                });

            Map(r => r.ChargeOwnerId)
                .Name("CHARGEOWNER")
                .Index(3);

            Map(r => r.ChargeCode)
                .Name("CHARGEID")
                .Index(4);

            Map(r => r.Quantity)
                .Name("CHARGEOCCURRENCES")
                .Index(5);

            Map(r => r.PeriodStart)
                .Name("PERIODSTART")
                .Index(6);

            Map(r => r.PeriodEnd)
                .Name("PERIODEND")
                .Index(7);

            if (actorInfo.MarketRole is MarketRole.DataHubAdministrator or MarketRole.SystemOperator)
            {
                Map(r => r.EnergySupplierId)
                    .Name("ENERGYSUPPLIERID")
                    .Index(8);
            }
        }
    }
}
