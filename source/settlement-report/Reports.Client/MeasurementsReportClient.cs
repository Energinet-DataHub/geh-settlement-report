using System.Net.Http.Json;
using System.Text;
using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;
using Newtonsoft.Json;

namespace Energinet.DataHub.Reports.Client;

// This class should preferably be internal. However, the BFF currently adds authentication headers to the
// HttpClient, which is required for the client to work. Can we find a way to use dependency injection and make this
// class internal?
public sealed class MeasurementsReportClient : IMeasurementsReportClient
{
    private const string BaseUrl = "measurements-reports";
    private readonly HttpClient _apiHttpClient;

    public MeasurementsReportClient(HttpClient apiHttpClient)
    {
        ArgumentNullException.ThrowIfNull(apiHttpClient);
        _apiHttpClient = apiHttpClient;
    }

    public Task<JobRunId> RequestAsync(MeasurementsReportRequestDto requestDto, CancellationToken cancellationToken)
    {
        return RequestAsync(requestDto, $"{BaseUrl}/request", cancellationToken);
    }

    public async Task<IEnumerable<RequestedMeasurementsReportDto>> GetAsync(CancellationToken cancellationToken)
    {
        using var requestApi = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/list");

        using var response = await _apiHttpClient.SendAsync(requestApi, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var responseApiContent = await response.Content.ReadFromJsonAsync<IEnumerable<RequestedMeasurementsReportDto>>(cancellationToken).ConfigureAwait(false) ?? [];

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

    private async Task<JobRunId> RequestAsync(object requestDto, string endpoint, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Content = new StringContent(
           JsonConvert.SerializeObject(requestDto),
           Encoding.UTF8,
           "application/json");

        using var response = await _apiHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var jobRunId = JsonConvert.DeserializeObject<long>(responseContent);
        return new JobRunId(jobRunId);
    }
}
