using Newtonsoft.Json;
using SmsGateway.Common.ApiClient;
using System.Net.Http.Json;
using System.Text;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TResponse?> GetAsync<TResponse>(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        catch (HttpRequestException ex)
        {
            // Log or handle error as needed
            Console.WriteLine($"GET request failed: {ex.Message}");
            return default;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonConvert.SerializeObject(data), encoding: null, "application/json")
        };

        using var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        return JsonConvert.DeserializeObject<TResponse>(content);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data, Dictionary<string, string>? headers)
    {

        var stringReq = JsonConvert.SerializeObject(data);
        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
        };

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<TResponse>(responseContent);
    }

    public async Task<TResponse?> PostAsync<TResponse>(string url, MultipartFormDataContent dataContent)
    {
        using var client = new HttpClient();

        var response = await client.PostAsync(url, dataContent);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<TResponse>(responseContent);
    }
}