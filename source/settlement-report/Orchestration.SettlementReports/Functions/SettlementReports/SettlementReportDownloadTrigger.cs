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

using System.Net;
using Azure;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.RevisionLog.Integration;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Security;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Telemetry;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using NodaTime;

namespace Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.Functions.SettlementReports;

internal sealed class SettlementReportDownloadTrigger
{
    private readonly IUserContext<FrontendUser> _userContext;
    private readonly ISettlementReportDownloadHandler _settlementReportDownloadHandler;
    private readonly IRevisionLogClient _revisionLogClient;

    public SettlementReportDownloadTrigger(
        IUserContext<FrontendUser> userContext,
        ISettlementReportDownloadHandler settlementReportDownloadHandler,
        IRevisionLogClient revisionLogClient)
    {
        _userContext = userContext;
        _settlementReportDownloadHandler = settlementReportDownloadHandler;
        _revisionLogClient = revisionLogClient;
    }

    [Function(nameof(SettlementReportDownload))]
    public async Task SettlementReportDownload(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        [FromBody] ReportRequestId reportRequestId,
        FunctionContext executionContext)
    {
        try
        {
            await _revisionLogClient.LogAsync(
                    new RevisionLogEntry(
                        actorId: _userContext.CurrentUser.Actor.ActorId,
                        userId: _userContext.CurrentUser.UserId,
                        logId: Guid.NewGuid(),
                        systemId: SubsystemInformation.Id,
                        occurredOn: SystemClock.Instance.GetCurrentInstant(),
                        activity: "SettlementReportDownload",
                        origin: nameof(SettlementReportDownloadTrigger),
                        payload: reportRequestId.Id))
                .ConfigureAwait(false);

            await _settlementReportDownloadHandler
                .DownloadReportAsync(
                    reportRequestId,
                    () =>
                    {
                        var response = req.CreateResponse(HttpStatusCode.OK);
                        response.Headers.Add("Content-Type", "application/octet-stream");
                        return response.Body;
                    },
                    _userContext.CurrentUser.Actor.ActorId,
                    _userContext.CurrentUser.MultiTenancy)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is InvalidOperationException or RequestFailedException)
        {
            _ = req.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}
