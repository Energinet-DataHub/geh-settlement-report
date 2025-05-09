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

using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Xunit.Abstractions;

namespace Energinet.DataHub.Reports.SubsystemTests.Fixtures;

public class SettlementReportFixture
{
    public SettlementReportFixture()
    {
        Logger = new TestDiagnosticsLogger();

        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri();

        SettlementReportClient = new SettlementReportClient(httpClient);
    }

    public TestDiagnosticsLogger Logger { get; }

    [NotNull]
    public SettlementReportRequestDto? SettlementReportRequestDto { get; set; }

    public SettlementReportClient SettlementReportClient { get; }

    public void SetTestOutputHelper(ITestOutputHelper? testOutputHelper)
    {
        Logger.TestOutputHelper = testOutputHelper;
    }
}
