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

using System.Globalization;
using CsvHelper;
using CsvHelper.TypeConversion;
using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.CalculationResults.Model;
using Energinet.DataHub.SettlementReport.Interfaces.CalculationResults.Model.EnergyResults;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;

namespace Energinet.DataHub.SettlementReport.Infrastructure.SettlementReports_v2.Generators;

public sealed class MeteringPointTimeSeriesFileGenerator : ISettlementReportFileGenerator
{
    private const int ChunkSize = 325_000; // About 31 rows per day, 251.875 rows in total.

    private readonly ISettlementReportMeteringPointTimeSeriesResultRepository _dataSource;
    private readonly Resolution _resolution;

    public MeteringPointTimeSeriesFileGenerator(ISettlementReportMeteringPointTimeSeriesResultRepository dataSource, Resolution resolution)
    {
        _dataSource = dataSource;
        _resolution = resolution;
    }

    public string FileExtension => ".csv";

    public async Task<int> CountChunksAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion)
    {
        var count = await _dataSource.CountAsync(filter, actorInfo, maximumCalculationVersion, _resolution).ConfigureAwait(false);
        return (int)Math.Ceiling(count / (double)ChunkSize);
    }

    public async Task WriteAsync(
        SettlementReportRequestFilterDto filter,
        SettlementReportRequestedByActor actorInfo,
        SettlementReportPartialFileInfo fileInfo,
        long maximumCalculationVersion,
        StreamWriter destination)
    {
        var csvHelper = new CsvWriter(destination, new CultureInfo(filter.CsvFormatLocale ?? "en-US"));
        var expectedQuantities = _resolution switch
        {
            Resolution.Hour => 25,
            Resolution.Quarter => 100,
            _ => throw new ArgumentException(nameof(_resolution)),
        };

        await using (csvHelper.ConfigureAwait(false))
        {
            csvHelper.Context.TypeConverterOptionsCache.AddOptions<decimal>(
                new TypeConverterOptions { Formats = ["0.000"], });

            if (fileInfo is { FileOffset: 0, ChunkOffset: 0 })
            {
                csvHelper.WriteField("METERINGPOINTID");
                csvHelper.WriteField("TYPEOFMP");
                csvHelper.WriteField("STARTDATETIME");

                for (var i = 0; i < expectedQuantities; ++i)
                {
                    csvHelper.WriteField($"ENERGYQUANTITY{i + 1}");
                }

                await csvHelper.NextRecordAsync().ConfigureAwait(false);
            }

            await foreach (var record in _dataSource.GetAsync(filter, actorInfo, maximumCalculationVersion, _resolution, fileInfo.ChunkOffset * ChunkSize, ChunkSize).ConfigureAwait(false))
            {
                csvHelper.WriteField(record.MeteringPointId, shouldQuote: true);
                csvHelper.WriteField(record.MeteringPointType switch
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
                    _ => throw new ArgumentOutOfRangeException(nameof(record.MeteringPointType)),
                });
                csvHelper.WriteField(record.StartDateTime);

                for (var i = 0; i < expectedQuantities; ++i)
                {
                    csvHelper.WriteField<decimal?>(record.Quantities.Count > i ? record.Quantities[i].Quantity : null);
                }

                await csvHelper.NextRecordAsync().ConfigureAwait(false);
            }
        }
    }
}
