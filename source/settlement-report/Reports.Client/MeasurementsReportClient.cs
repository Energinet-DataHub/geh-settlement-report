using System.Net.Http.Json;
using System.Text;
using Energinet.DataHub.Reports.Interfaces.Models;
using Energinet.DataHub.Reports.Interfaces.Models.MeasurementsReport;
using Newtonsoft.Json;

namespace Energinet.DataHub.Reports.Client;

internal sealed class MeasurementsReportClient : IMeasurementsReportClient
{
    private readonly HttpClient _apiHttpClient;

    public MeasurementsReportClient(HttpClient apiHttpClient)
    {
        ArgumentNullException.ThrowIfNull(apiHttpClient);
        _apiHttpClient = apiHttpClient;
    }

    public Task<JobRunId> RequestAsync(MeasurementsReportRequestDto requestDto, CancellationToken cancellationToken)
    {
        return RequestAsync(requestDto, "measurements-reports/request", cancellationToken);
    }

    public async Task<IEnumerable<RequestedMeasurementsReportDto>> GetMeasurementsReportAsync(CancellationToken cancellationToken)
    {
        using var requestApi = new HttpRequestMessage(HttpMethod.Get, "measurements-reports/list");

        using var response = await _apiHttpClient.SendAsync(requestApi, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var responseApiContent = await response.Content.ReadFromJsonAsync<IEnumerable<RequestedMeasurementsReportDto>>(cancellationToken).ConfigureAwait(false) ?? [];

        return responseApiContent.OrderByDescending(x => x.CreatedDateTime);
    }

    public async Task<Stream> DownloadAsync(ReportRequestId requestId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "measurements-reports/download");
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
        using var request = new HttpRequestMessage(HttpMethod.Post, "measurements-reports/cancel");

        request.Content = new StringContent(
            JsonConvert.SerializeObject(requestId),
            Encoding.UTF8,
            "application/json");

        using var responseMessage = await _apiHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        responseMessage.EnsureSuccessStatusCode();
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
