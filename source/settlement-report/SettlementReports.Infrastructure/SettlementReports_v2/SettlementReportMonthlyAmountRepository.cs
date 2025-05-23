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

public sealed class SettlementReportMonthlyAmountRepository : ISettlementReportMonthlyAmountRepository
{
    private readonly ISettlementReportDatabricksContext _settlementReportDatabricksContext;

    public SettlementReportMonthlyAmountRepository(ISettlementReportDatabricksContext settlementReportDatabricksContext)
    {
        _settlementReportDatabricksContext = settlementReportDatabricksContext;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo)
    {
        return Task.FromResult<int>(1);
    }

    public async IAsyncEnumerable<SettlementReportMonthlyAmountRow> GetAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, int skip, int take)
    {
        var view = ApplyFilter(_settlementReportDatabricksContext.MonthlyAmountsView, filter, actorInfo);

        await foreach (var row in view.AsAsyncEnumerable().ConfigureAwait(false))
        {
            yield return new SettlementReportMonthlyAmountRow(
                CalculationTypeMapper.FromDeltaTableValue(row.CalculationType),
                row.GridAreaCode,
                row.EnergySupplierId,
                row.Time,
                row.QuantityUnit is null ? null : QuantityUnitMapper.FromDeltaTableValue(row.QuantityUnit),
                row.Amount,
                row.ChargeType is null ? null : ChargeTypeMapper.FromDeltaTableValue(row.ChargeType),
                row.ChargeCode,
                row.ChargeOwnerId);
        }
    }

    private static IQueryable<SettlementReportMonthlyAmountsViewEntity> ApplyFilter(
        IQueryable<SettlementReportMonthlyAmountsViewEntity> source,
        SettlementReportRequestFilterDto filter,
        SettlementReportRequestedByActor actorInfo)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.Time >= filter.PeriodStart.ToInstant())
            .Where(row => row.Time < filter.PeriodEnd.ToInstant())
            .Where(row => row.ChargeOwnerId != null)
            .Where(row => row.ChargeCode != null)
            .Where(row => row.ChargeType != null)
            .Where(row => row.IsTax != null)
            .Where(row => row.CalculationId == calculationId!.Id);

        switch (actorInfo.MarketRole)
        {
            case MarketRole.SystemOperator:
                source = source.Where(row =>
                    row.IsTax == false &&
                    row.ChargeOwnerId == actorInfo.ChargeOwnerId);
                break;
            case MarketRole.GridAccessProvider:
                source = source.Where(row =>
                    row.IsTax == true ||
                    row.ChargeOwnerId == actorInfo.ChargeOwnerId);
                break;
        }

        if (!string.IsNullOrWhiteSpace(filter.EnergySupplier))
        {
            source = source.Where(row => row.EnergySupplierId == filter.EnergySupplier);
        }

        return source;
    }
}
