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

using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.SettlementReport;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.Fixtures;

// TODO JMG: Make this a real client including nuget, so it can be shared with BFF?
// TODO BJM: Make this reusable for all report types

/// <summary>
/// Interface of client for working with the settlement reports.
/// </summary>
public interface ISettlementReportClient
{
    /// <summary>
    /// Requests generation of a new settlement report.
    /// </summary>
    /// <returns>The job id.</returns>
    public Task<JobRunId> RequestAsync(SettlementReportRequestDto requestDto, CancellationToken cancellationToken);

    /// <summary>
    /// Requests generation of a new settlement report.
    /// </summary>
    /// <returns>The job id.</returns>
    public Task<JobRunId> RequestAsync(MeasurementsReportRequestDto requestDto, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of all settlement reports visible to the current user.
    /// </summary>
    /// <returns>A list of settlement reports.</returns>
    public Task<IEnumerable<RequestedSettlementReportDto>> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Downloads the settlement report with the specified id.
    /// </summary>
    /// <returns>The stream to the report.</returns>
    public Task<Stream> DownloadAsync(ReportRequestId requestId, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels the settlement report with the specified id.
    /// </summary>
    public Task CancelAsync(ReportRequestId requestId, CancellationToken cancellationToken);
}
