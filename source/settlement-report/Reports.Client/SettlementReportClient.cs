using System.Net.Http.Json;
using System.Text;
using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;
using Newtonsoft.Json;

namespace Energinet.DataHub.Reports.Client;

internal sealed class SettlementReportClient : ISettlementReportClient
{
    private const string BaseUrl = "settlement-reports";
    private readonly HttpClient _apiHttpClient;

    public SettlementReportClient(HttpClient apiHttpClient)
    {
        ArgumentNullException.ThrowIfNull(apiHttpClient);
        _apiHttpClient = apiHttpClient;
    }

    public Task<JobRunId> RequestAsync(SettlementReportRequestDto requestDto, CancellationToken cancellationToken)
    {
        return IsPeriodAcrossMonths(requestDto.Filter)
            ? throw new ArgumentException("Invalid period, start date and end date should be within same month", nameof(requestDto))
            : RequestAsync(requestDto, $"{BaseUrl}/RequestSettlementReport", cancellationToken);
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> GetAsync(CancellationToken cancellationToken)
    {
        using var requestApi = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/list");

        using var response = await _apiHttpClient.SendAsync(requestApi, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var responseApiContent = await response.Content.ReadFromJsonAsync<IEnumerable<RequestedSettlementReportDto>>(cancellationToken).ConfigureAwait(false) ?? [];

        return responseApiContent.OrderByDescending(x => x.CreatedDateTime);
    }

    public async Task<Stream> DownloadAsync(ReportRequestId requestId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/download");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(requestId),
            Encoding.UTF8,
            "application/json");

        var response = await _apiHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task CancelAsync(ReportRequestId requestId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/cancel");

        request.Content = new StringContent(
            JsonConvert.SerializeObject(requestId),
            Encoding.UTF8,
            "application/json");

        using var responseMessage = await _apiHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
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

        using var response = await _apiHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request to {endpoint} failed with status code {response.StatusCode}: {responseText}.");
        }

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var jobRunId = JsonConvert.DeserializeObject<long>(responseContent);
        return new JobRunId(jobRunId);
    }
}
