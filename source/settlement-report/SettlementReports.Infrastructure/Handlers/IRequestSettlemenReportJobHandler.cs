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

namespace Energinet.DataHub.SettlementReport.Infrastructure.Handlers;

public interface IRequestSettlemenReportJobHandler
{
    /// <summary>
    /// Request a settlement report job
    /// </summary>
    /// <param name="command"></param>
    /// <param name="userId"></param>
    /// <param name="actorId"></param>
    /// <param name="isFas"></param>
    /// <returns>A long value representing the job id of the requested settlement report.</returns>
    Task<long> HandleAsync(SettlementReportRequestDto command, Guid userId, Guid actorId, bool isFas);
}
