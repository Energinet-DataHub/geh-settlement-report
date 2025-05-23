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
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;

namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;

// TODO BJM: This is a vague name not conveying the purpose of the interface.
//           Segregate a factory instead and then replace usages with both using the factory and the repository directly?
public interface ISettlementReportInitializeHandler
{
    Task InitializeAsync(
        Guid userId,
        Guid actorId,
        bool hideReport,
        ReportRequestId requestId,
        SettlementReportRequestDto request);

    Task InitializeFromJobAsync(
        Guid userId,
        Guid actorId,
        bool hideReport,
        JobRunId jobId,
        ReportRequestId requestId,
        SettlementReportRequestDto request);
}
