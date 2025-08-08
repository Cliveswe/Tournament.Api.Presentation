// Ignore Spelling: json
using System.Net.Http.Headers;
using System.Text.Json;

namespace Tournaments.Client.Clients;

public class TournamentsClient : ITournamentsClient
{
    private const string json = "application/json";
    private HttpClient client;

    public TournamentsClient(HttpClient httpClient)
    {
        client = httpClient;
        client.BaseAddress = new Uri("https://localhost:7225");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(json));
        client.Timeout = new TimeSpan(0, 0, 5);//Timeout in 5 seconds.
    }

    public async Task<T> GetAsync<T>(string path, string contentType = json)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,path);
        HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        string stream = await response.Content.ReadAsStringAsync();
        T? result = DeserializeApiResponse<T>(stream);
        return result!;
    }

    /// <summary>
    /// Deserializes a JSON string into the specified .NET type using camelCase property naming.
    /// </summary>
    /// <typeparam name="TResult">
    /// The target .NET type to deserialize the JSON into, such as a DTO or a collection of DTOs.
    /// </typeparam>
    /// <param name="result">The JSON string to deserialize.</param>
    /// <returns>
    /// An instance of <typeparamref name="TResult"/> if deserialization is successful; otherwise, <c>null</c>.
    /// </returns>
    private static TResult? DeserializeApiResponse<TResult>(string result)
    {
        return JsonSerializer
            .Deserialize<TResult>(result,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
    }

}
