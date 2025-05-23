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
using Energinet.DataHub.SettlementReport.Application.SettlementReports.Commands;
using Energinet.DataHub.SettlementReport.Interfaces.Helpers;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Application.SettlementReports.Handlers;

public sealed class CancelSettlementReportJobHandler : ICancelSettlementReportJobHandler
{
    private readonly ISettlementReportDatabricksJobsHelper _jobHelper;
    private readonly ISettlementReportRepository _repository;

    public CancelSettlementReportJobHandler(
        ISettlementReportDatabricksJobsHelper jobHelper,
        ISettlementReportRepository repository)
    {
        _jobHelper = jobHelper;
        _repository = repository;
    }

    public async Task HandleAsync(CancelSettlementReportCommand cancelSettlementReportCommand)
    {
        var report = await _repository
            .GetAsync(cancelSettlementReportCommand.RequestId.Id)
            .ConfigureAwait(false) ?? throw new InvalidOperationException("Report not found.");

        if (!report.JobId.HasValue)
        {
            throw new InvalidOperationException("Report does not have a JobId.");
        }

        if (report.UserId != cancelSettlementReportCommand.UserId)
        {
            throw new InvalidOperationException("UserId does not match. Only the user that started the report can cancel it.");
        }

        if (report.Status is not ReportStatus.InProgress)
        {
            throw new InvalidOperationException($"Can't cancel a report with status: {report.Status}");
        }

        await _jobHelper.CancelAsync(report.JobId.Value).ConfigureAwait(false);
    }
}
