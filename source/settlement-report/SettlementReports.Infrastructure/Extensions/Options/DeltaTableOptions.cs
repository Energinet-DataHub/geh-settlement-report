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

using System.ComponentModel.DataAnnotations;

namespace Energinet.DataHub.SettlementReport.Common.Infrastructure.Options;

public class DeltaTableOptions
{
    /// <summary>
    /// Name of the schema/database under which the output tables are associated.
    /// </summary>
    public string SCHEMA_NAME { get; set; } = "wholesale_output";

    /// <summary>
    /// Name of the schema/database under which the basis data tables are associated.
    /// </summary>
    public string BasisDataSchemaName { get; set; } = "basis_data";

    /// <summary>
    /// The source where the different databricks views are located, consisting of the catalog and schema name.
    /// </summary>
    public string SettlementReportViewsSource => $"{DatabricksCatalogName}.{SettlementReportSchemaName}";

    /// <summary>
    /// Name of the catalog in which the databricks views are located.
    /// Should point at the unity catalog when running in Azure, and use hive_metastore to be able to populate testsdata when running tests
    /// </summary>
    [Required]
    public string DatabricksCatalogName { get; set; } = null!;

    /// <summary>
    /// Name of the schema/database under which the settlement report views are associated.
    /// </summary>
    public string SettlementReportSchemaName { get; set; } = "wholesale_settlement_reports";

    public string WHOLESALE_RESULTS_V1_VIEW_NAME { get; set; } = "amounts_per_charge_v1";

    public string ENERGY_RESULTS_POINTS_PER_GA_V1_VIEW_NAME { get; set; } = "energy_v1";

    public string ENERGY_RESULTS_POINTS_PER_ES_GA_V1_VIEW_NAME { get; set; } = "energy_per_es_v1";

    public string ENERGY_RESULTS_METERING_POINT_TIME_SERIES_V1_VIEW_NAME { get; set; } = "metering_point_time_series_v1";

    public string CHARGE_LINK_PERIODS_V1_VIEW_NAME { get; set; } = "charge_link_periods_v1";

    public string METERING_POINT_MASTER_DATA_V1_VIEW_NAME { get; set; } = "metering_point_periods_v1";

    public string MONTHLY_AMOUNTS_V1_VIEW_NAME { get; set; } = "monthly_amounts_v1";

    public string CHARGE_PRICES_V1_VIEW_NAME { get; set; } = "charge_prices_v1";

    public string MONTHLY_AMOUNTS_PER_CHARGE_V1_VIEW_NAME { get; set; } = "monthly_amounts_per_charge_v1";

    public string TOTAL_MONTHLY_AMOUNTS_V1_VIEW_NAME { get; set; } = "total_monthly_amounts_v1";
}