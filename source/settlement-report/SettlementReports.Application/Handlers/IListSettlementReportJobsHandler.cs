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

using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.SettlementReport;

namespace Energinet.DataHub.SettlementReport.Application.Handlers;

/// <summary>
/// The handler for listing settlement report jobs.
/// </summary>
public interface IListSettlementReportJobsHandler
{
    /// <summary>
    /// List all settlement report jobs
    /// </summary>
    /// <returns>A list of settlement reports with metadata.</returns>
    Task<IEnumerable<RequestedSettlementReportDto>> HandleAsync();

    /// <summary>
    /// List all settlement report jobs for a given actor
    /// </summary>
    /// <param name="actorId">The actorId to return reports for</param>
    /// <returns>A list of settlement reports with metadata for a specific actor.</returns>
    Task<IEnumerable<RequestedSettlementReportDto>> HandleAsync(Guid actorId);
}
