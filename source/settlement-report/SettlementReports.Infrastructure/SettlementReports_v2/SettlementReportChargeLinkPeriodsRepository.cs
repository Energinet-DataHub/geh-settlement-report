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

using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Infrastructure.Persistence.Databricks;
using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;
using Microsoft.EntityFrameworkCore;
using NodaTime.Extensions;

namespace Energinet.DataHub.SettlementReport.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportChargeLinkPeriodsRepository : ISettlementReportChargeLinkPeriodsRepository
{
    private readonly ISettlementReportDatabricksContext _settlementReportDatabricksContext;

    public SettlementReportChargeLinkPeriodsRepository(ISettlementReportDatabricksContext settlementReportDatabricksContext)
    {
        _settlementReportDatabricksContext = settlementReportDatabricksContext;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo)
    {
        return Task.FromResult<int>(1);
    }

    public async IAsyncEnumerable<SettlementReportChargeLinkPeriodsResultRow> GetAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, int skip, int take)
    {
        var view = ApplyFilter(_settlementReportDatabricksContext.ChargeLinkPeriodsView, filter, actorInfo);

        await foreach (var row in view.AsAsyncEnumerable().ConfigureAwait(false))
        {
            yield return new SettlementReportChargeLinkPeriodsResultRow(
                row.MeteringPointId,
                MeteringPointTypeMapper.FromDeltaTableValueNonNull(row.MeteringPointType),
                ChargeTypeMapper.FromDeltaTableValue(row.ChargeType),
                row.ChargeOwnerId,
                row.ChargeCode,
                row.Quantity,
                row.FromDate,
                row.ToDate,
                row.EnergySupplierId);
        }
    }

    private static IQueryable<SettlementReportChargeLinkPeriodsViewEntity> ApplyFilter(
        IQueryable<SettlementReportChargeLinkPeriodsViewEntity> source,
        SettlementReportRequestFilterDto filter,
        SettlementReportRequestedByActor actorInfo)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.FromDate <= filter.PeriodEnd.ToInstant())
            .Where(row => row.ToDate >= filter.PeriodStart.ToInstant())
            .Where(row => row.CalculationId == calculationId!.Id);

        if (actorInfo.MarketRole == MarketRole.SystemOperator)
        {
            source = source.Where(row =>
                row.IsTax == false &&
                row.ChargeOwnerId == actorInfo.ChargeOwnerId);
        }

        if (actorInfo.MarketRole == MarketRole.GridAccessProvider)
        {
            source = source.Where(row =>
                row.IsTax == true ||
                row.ChargeOwnerId == actorInfo.ChargeOwnerId);
        }

        if (!string.IsNullOrWhiteSpace(filter.EnergySupplier))
        {
            source = source.Where(row => row.EnergySupplierId == filter.EnergySupplier);
        }

        return source;
    }
}
