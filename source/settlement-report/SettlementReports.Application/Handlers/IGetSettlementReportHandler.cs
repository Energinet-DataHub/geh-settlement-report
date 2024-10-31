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

using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Application.Handlers;

/// <summary>
/// The handler for getting a settlement report.
/// </summary>
public interface IGetSettlementReportHandler
{
    /// <summary>
    /// Gets a settlement report while disregarding actor access, is used by FAS/Multitenancy.
    /// </summary>
    /// <returns>A list of settlement reports with metadata for a specific actor.</returns>
    Task<RequestedSettlementReportDto> HandleAsync();

    /// <summary>
    /// Gets a settlement report respecting actor access.
    /// </summary>
    /// <param name="actorId">The actorId to return reports for</param>
    /// <returns>A list of settlement reports with metadata for a specific actor.</returns>
    Task<RequestedSettlementReportDto> HandleAsync(Guid actorId);
}
