using System.Net.Http.Json;
using System.Text;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;
using GraphQL.Types;
using Newtonsoft.Json;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.Fixtures;

// TODO JMG: Make this a real client including nuget, so it can be shared with BFF?
internal sealed class SettlementReportClient : ISettlementReportClient
{
    private readonly HttpClient _apiHttpClient;

    public SettlementReportClient(HttpClient apiHttpClient)
    {
        ArgumentNullException.ThrowIfNull(apiHttpClient);
        _apiHttpClient = apiHttpClient;
    }

    public Task<JobRunId> RequestAsync(SettlementReportRequestDto requestDto, CancellationToken cancellationToken)
    {
        if (IsPeriodAcrossMonths(requestDto.Filter))
            throw new ArgumentException("Invalid period, start date and end date should be within same month", nameof(requestDto));

        return RequestAsync(requestDto, "settlement-reports/RequestSettlementReport2", cancellationToken);
    }

    public Task<JobRunId> RequestAsync(MeasurementsReportRequestDto requestDto, CancellationToken cancellationToken)
    {
        return RequestAsync(requestDto, "measurements-reports/request", cancellationToken);
    }

    public async Task<IEnumerable<RequestedMeasurementsReportDto>> GetMeasurementsReportAsync(CancellationToken cancellationToken)
    {
        using var requestApi = new HttpRequestMessage(HttpMethod.Get, "measurements-reports/list");

        using var response = await _apiHttpClient.SendAsync(requestApi, cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseApiContent = await response.Content.ReadFromJsonAsync<IEnumerable<RequestedMeasurementsReportDto>>(cancellationToken) ?? [];

        return responseApiContent.OrderByDescending(x => x.CreatedDateTime);
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> GetAsync(CancellationToken cancellationToken)
    {
        using var requestApi = new HttpRequestMessage(HttpMethod.Get, "settlement-reports/list");

        using var response = await _apiHttpClient.SendAsync(requestApi, cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseApiContent = await response.Content.ReadFromJsonAsync<IEnumerable<RequestedSettlementReportDto>>(cancellationToken) ?? [];

        return responseApiContent.OrderByDescending(x => x.CreatedDateTime);
    }

    public async Task<Stream> DownloadAsync(ReportRequestId requestId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "settlement-reports/download");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(requestId),
            Encoding.UTF8,
            "application/json");

        var response = await _apiHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task CancelAsync(ReportRequestId requestId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "settlement-reports/cancel");

        request.Content = new StringContent(
            JsonConvert.SerializeObject(requestId),
            Encoding.UTF8,
            "application/json");

        using var responseMessage = await _apiHttpClient.SendAsync(request, cancellationToken);
        responseMessage.EnsureSuccessStatusCode();
    }

    private static bool IsPeriodAcrossMonths(SettlementReportRequestFilterDto settlementReportRequestFilter)
    {
        var startDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(settlementReportRequestFilter.PeriodStart, "Romance Standard Time");
        var endDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(settlementReportRequestFilter.PeriodEnd.AddMilliseconds(-1), "Romance Standard Time");
        return startDate.Month != endDate.Month
            || startDate.Year != endDate.Year;
    }

    private async Task<JobRunId> RequestAsync(object requestDto, string endpoint, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Content = new StringContent(
           JsonConvert.SerializeObject(requestDto),
           Encoding.UTF8,
           "application/json");

        using var response = await _apiHttpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}: {responseText}.");
        }

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var jobRunId = JsonConvert.DeserializeObject<long>(responseContent);
        return new JobRunId(jobRunId);
    }
}
