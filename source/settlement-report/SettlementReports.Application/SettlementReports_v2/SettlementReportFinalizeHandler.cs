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
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;
using NodaTime;

namespace Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

public sealed class SettlementReportFinalizeHandler : ISettlementReportFinalizeHandler
{
    private readonly IClock _clock;
    private readonly ISettlementReportFileRepository _fileRepository;
    private readonly ISettlementReportRepository _repository;

    public SettlementReportFinalizeHandler(
        ISettlementReportFileRepository fileRepository,
        ISettlementReportRepository repository,
        IClock clock)
    {
        _fileRepository = fileRepository;
        _repository = repository;
        _clock = clock;
    }

    public async Task FinalizeAsync(GeneratedSettlementReportDto generatedReport)
    {
        foreach (var file in generatedReport.TemporaryFiles)
        {
            await _fileRepository
                .DeleteAsync(file.RequestId, file.StorageFileName)
                .ConfigureAwait(false);
        }

        var request = await _repository
            .GetAsync(generatedReport.RequestId.Id)
            .ConfigureAwait(false);

        request.MarkAsCompleted(_clock, generatedReport);

        await _repository
            .AddOrUpdateAsync(request)
            .ConfigureAwait(false);
    }
}
