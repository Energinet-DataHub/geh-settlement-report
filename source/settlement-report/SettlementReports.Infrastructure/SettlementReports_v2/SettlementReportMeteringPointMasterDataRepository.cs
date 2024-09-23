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

using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Infrastructure.Persistence.Databricks;
using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.SettlementReport.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime.Extensions;
using DbFunctions = Energinet.DataHub.SettlementReport.Infrastructure.Experimental.DatabricksSqlQueryableExtensions.Functions;

namespace Energinet.DataHub.SettlementReport.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportMeteringPointMasterDataRepository : ISettlementReportMeteringPointMasterDataRepository
{
    private readonly ISettlementReportDatabricksContext _settlementReportDatabricksContext;

    public SettlementReportMeteringPointMasterDataRepository(ISettlementReportDatabricksContext settlementReportDatabricksContext)
    {
        _settlementReportDatabricksContext = settlementReportDatabricksContext;
    }

    public Task<int> CountAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, long maximumCalculationVersion)
    {
        return Task.FromResult<int>(1);
    }

    public async IAsyncEnumerable<SettlementReportMeteringPointMasterDataRow> GetAsync(SettlementReportRequestFilterDto filter, SettlementReportRequestedByActor actorInfo, int skip, int take, long maximumCalculationVersion)
    {
        var view = ApplyFilter(_settlementReportDatabricksContext.SettlementReportMeteringPointMasterDataView, filter);
        var query = filter.CalculationType == CalculationType.BalanceFixing
            ? filter.EnergySupplier is not null
                ? GetForBalanceFixingPerEnergySupplier(skip, take, view, ApplyFilter(_settlementReportDatabricksContext.EnergyResultPointsPerEnergySupplierGridAreaView, filter, maximumCalculationVersion))
                : GetForBalanceFixing(view, ApplyFilter(_settlementReportDatabricksContext.EnergyResultPointsPerGridAreaView, filter, maximumCalculationVersion))
            : GetForNonBalanceFixing(view, actorInfo);

        await foreach (var row in query.AsAsyncEnumerable().ConfigureAwait(false))
        {
            var rowGridAreaFromCode = row.GridAreaFromCode;
            var rowGridAreaToCode = row.GridAreaToCode;

            if (actorInfo.MarketRole == MarketRole.SystemOperator)
            {
                rowGridAreaFromCode = null;
                rowGridAreaToCode = null;
            }

            yield return new SettlementReportMeteringPointMasterDataRow(
                row.MeteringPointId,
                MeteringPointTypeMapper.FromDeltaTableValue(row.MeteringPointType),
                row.GridAreaCode,
                rowGridAreaFromCode,
                rowGridAreaToCode,
                SettlementMethodMapper.FromDeltaTableValue(row.SettlementMethod),
                row.EnergySupplierId,
                row.FromDate,
                row.ToDate);
        }
    }

    private IQueryable<SettlementReportMeteringPointMasterDataViewEntity> GetForNonBalanceFixing(IQueryable<SettlementReportMeteringPointMasterDataViewEntity> view, SettlementReportRequestedByActor actorInfo)
    {
        if (actorInfo.MarketRole == MarketRole.SystemOperator)
        {
            var systemOperatorMeteringPoints = GetSystemOperatorMeteringPoints(actorInfo);
            view = view.Join(
                systemOperatorMeteringPoints,
                outer => outer.MeteringPointId,
                inner => inner,
                (outer, _) => outer);
        }

        return view;
    }

    private static IQueryable<SettlementReportMeteringPointMasterDataViewEntity> GetForBalanceFixing(
        IQueryable<SettlementReportMeteringPointMasterDataViewEntity> view,
        IQueryable<SettlementReportEnergyResultPointsPerGridAreaViewEntity> viewForLatest)
    {
        var latestJoin = viewForLatest
            .GroupBy(row =>
                DbFunctions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"))
            .Select(group => new { day = group.Key, max_calc_version = group.Max(row => row.CalculationVersion), });

        var latestCalcIdForMetering = viewForLatest
            .Join(
                latestJoin,
                outer => new
                {
                    day = DbFunctions.ToStartOfDayInTimeZone(
                            outer.Time,
                            "Europe/Copenhagen"),
                    max_calc_version = outer.CalculationVersion,
                },
                inner => inner,
                (outer, inner) => new { outer.CalculationId })
            .Distinct();

        var query = (from m in view
                     join l in latestCalcIdForMetering on m.CalculationId equals l.CalculationId
                     select m)
            .Distinct()
            .OrderBy(row => row.MeteringPointId);

        return query;
    }

    private static IQueryable<SettlementReportMeteringPointMasterDataViewEntity> GetForBalanceFixingPerEnergySupplier(
        int skip,
        int take,
        IQueryable<SettlementReportMeteringPointMasterDataViewEntity> view,
        IQueryable<SettlementReportEnergyResultPointsPerEnergySupplierGridAreaViewEntity> viewForLatest)
    {
        var latestJoin = viewForLatest
            .GroupBy(row =>
                DbFunctions.ToStartOfDayInTimeZone(row.Time, "Europe/Copenhagen"))
            .Select(group => new { day = group.Key, max_calc_version = group.Max(row => row.CalculationVersion), });

        var latestCalcIdForMetering = viewForLatest
            .Join(
                latestJoin,
                outer => new
                {
                    day = DbFunctions.ToStartOfDayInTimeZone(
                            outer.Time,
                            "Europe/Copenhagen"),
                    max_calc_version = outer.CalculationVersion,
                },
                inner => inner,
                (outer, inner) => new { outer.CalculationId })
            .Distinct();

        var query = (from m in view
                     join l in latestCalcIdForMetering on m.CalculationId equals l.CalculationId
                     select m)
            .Distinct()
            .OrderBy(row => row.MeteringPointId);

        return query;
    }

    private static IQueryable<SettlementReportMeteringPointMasterDataViewEntity> ApplyFilter(
        IQueryable<SettlementReportMeteringPointMasterDataViewEntity> source,
        SettlementReportRequestFilterDto filter)
    {
        var (gridAreaCode, calculationId) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.FromDate <= filter.PeriodEnd.ToInstant())
            .Where(row => row.ToDate >= filter.PeriodStart.ToInstant());

        if (filter.CalculationType != CalculationType.BalanceFixing)
        {
            source = source.Where(row => row.CalculationId == calculationId!.Id);
        }

        if (!string.IsNullOrWhiteSpace(filter.EnergySupplier))
        {
            source = source.Where(row => row.EnergySupplierId == filter.EnergySupplier);
        }

        return source;
    }

    private static IQueryable<SettlementReportEnergyResultPointsPerGridAreaViewEntity> ApplyFilter(
        IQueryable<SettlementReportEnergyResultPointsPerGridAreaViewEntity> source,
        SettlementReportRequestFilterDto filter,
        long maximumCalculationVersion)
    {
        var (gridAreaCode, _) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.Time >= filter.PeriodStart.ToInstant())
            .Where(row => row.Time < filter.PeriodEnd.ToInstant())
            .Where(row => row.CalculationVersion <= maximumCalculationVersion);

        return source;
    }

    private static IQueryable<SettlementReportEnergyResultPointsPerEnergySupplierGridAreaViewEntity> ApplyFilter(
        IQueryable<SettlementReportEnergyResultPointsPerEnergySupplierGridAreaViewEntity> source,
        SettlementReportRequestFilterDto filter,
        long maximumCalculationVersion)
    {
        var (gridAreaCode, _) = filter.GridAreas.Single();

        source = source
            .Where(row => row.GridAreaCode == gridAreaCode)
            .Where(row => row.CalculationType == CalculationTypeMapper.ToDeltaTableValue(filter.CalculationType))
            .Where(row => row.Time >= filter.PeriodStart.ToInstant())
            .Where(row => row.Time < filter.PeriodEnd.ToInstant())
            .Where(row => row.CalculationVersion <= maximumCalculationVersion);

        if (!string.IsNullOrWhiteSpace(filter.EnergySupplier))
        {
            source = source.Where(row => row.EnergySupplierId == filter.EnergySupplier);
        }

        return source;
    }

    private IQueryable<string> GetSystemOperatorMeteringPoints(SettlementReportRequestedByActor actorInfo)
    {
        return _settlementReportDatabricksContext
            .ChargeLinkPeriodsView
            .Where(row => row.IsTax == false && row.ChargeOwnerId == actorInfo.ChargeOwnerId)
            .Select(row => row.MeteringPointId)
            .Distinct();
    }
}
