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

using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using NodaTime;

namespace Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

public sealed class SettlementReportInitializeHandler : ISettlementReportInitializeHandler
{
    private readonly ISettlementReportRepository _repository;

    public SettlementReportInitializeHandler(ISettlementReportRepository repository)
    {
        _repository = repository;
    }

    public Task InitializeAsync(
        Guid userId,
        Guid actorId,
        bool hideReport,
        SettlementReportRequestId requestId,
        SettlementReportRequestDto request)
    {
        var settlementReport = new SettlementReport(SystemClock.Instance, userId, actorId, hideReport, requestId, request);
        return _repository.AddOrUpdateAsync(settlementReport);
    }
}