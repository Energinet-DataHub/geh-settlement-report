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
using Energinet.DataHub.SettlementReport.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;

namespace Energinet.DataHub.SettlementReport.Infrastructure.SettlementReports_v2.Generators;

public sealed class EnergyResultFileGenerator : CsvFileGeneratorBase<SettlementReportEnergyResultRow, EnergyResultFileGenerator.SettlementReportEnergyResultRowMap>
{
    private readonly ISettlementReportEnergyResultRepository _dataSource;

    public EnergyResultFileGenerator(ISettlementReportEnergyResultRepository dataSource)
        : base(
            int.MaxValue, //350, // Up to 31 * 24 * 4 rows in each chunk for a month, 1.041.600 rows per chunk in total.
            quotedColumns: [0, 7])
    {
        _dataSource = dataSource;
    }

    protected override Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion)
    {
        return _dataSource.CountAsync(filter, actorInfo, maximumCalculationVersion);
    }

    protected override IAsyncEnumerable<SettlementReportEnergyResultRow> GetAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion, int skipChunks, int takeChunks)
    {
        return _dataSource.GetAsync(filter, actorInfo, maximumCalculationVersion, skipChunks, takeChunks);
    }

    protected override void RegisterClassMap(CsvWriter csvHelper, SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo)
    {
        csvHelper.Context.RegisterClassMap(new SettlementReportEnergyResultRowMap(actorInfo));
    }

    public sealed class SettlementReportEnergyResultRowMap : ClassMap<SettlementReportEnergyResultRow>
    {
        public SettlementReportEnergyResultRowMap(SettlementReportRequestedByActor actorInfo)
        {
            Map(r => r.GridAreaCode)
                .Name("METERINGGRIDAREAID")
                .Index(0)
                .Convert(row => row.Value.GridAreaCode.PadLeft(3, '0'));

            Map(r => r.EnergyBusinessProcess)
                .Name("ENERGYBUSINESSPROCESS")
                .Index(1);

            Map(r => r.Time)
                .Name("STARTDATETIME")
                .Index(2);

            Map(r => r.Resolution)
                .Name("RESOLUTIONDURATION")
                .Index(3)
                .Convert(row => row.Value.Resolution switch
                {
                    Resolution.Hour => "PT1H",
                    Resolution.Quarter => "PT15M",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.Resolution)),
                });

            Map(r => r.MeteringPointType)
                .Name("TYPEOFMP")
                .Index(4)
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
                .Index(5)
                .Convert(row => row.Value.SettlementMethod switch
                {
                    null => string.Empty,
                    SettlementMethod.NonProfiled => "E02",
                    SettlementMethod.Flex => "D01",
                    _ => throw new ArgumentOutOfRangeException(nameof(row.Value.SettlementMethod)),
                });

            Map(r => r.Quantity)
                .Name("ENERGYQUANTITY")
                .Index(6)
                .Data.TypeConverterOptions.Formats = ["0.000"];

            if (actorInfo.MarketRole is MarketRole.DataHubAdministrator)
            {
                Map(r => r.EnergySupplierId)
                    .Name("ENERGYSUPPLIERID")
                    .Index(7);
            }
        }
    }
}
