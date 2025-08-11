// Ignore Spelling: json Deserializes deserialize deserialization
using System.Net.Http.Headers;
using System.Text.Json;

namespace Tournaments.Client.Clients;

public class TournamentsClient : ITournamentsClient
{
    private const string json = "application/json";
    private HttpClient client;

    /// <summary>
    /// Unify both serialization and deserialization so they share the same JsonSerializerOptions,
    /// which prevents mismatches.
    /// </summary>
    private static readonly JsonSerializerOptions CamelCaseOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };


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
        T? result = DeserializeFromJson<T>(stream);
        return result!;
    }

    public async Task<TResult?> PostAsync<TCreate, TResult>(string path, Func<TCreate> payloadFactory, string contentType = json)
    {
        if(payloadFactory == null)
            throw new ArgumentNullException(nameof(payloadFactory), "Payload factory cannot be null.");

        // Create request
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, path);

        // Build payload using factory delegate
        TCreate payload = payloadFactory();
        string jsonPayload = SerializeToJson(payload);

        request.Content = new StringContent(jsonPayload);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using HttpResponseMessage response = await client.SendAsync(request);

        try {

            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();
            return DeserializeFromJson<TResult>(result);

        } catch(Exception ex) {
            _ = ex;// silence compiler if unused
            return default;
        }
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
    //private static TResult? DeserializeApiResponse<TResult>(string result)
    //{
    //    return JsonSerializer
    //        .Deserialize<TResult>(result,
    //        new JsonSerializerOptions
    //        {
    //            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    //        });
    //}

    private static TResult? DeserializeFromJson<TResult>(string jsonString) =>
        JsonSerializer.Deserialize<TResult>(jsonString, CamelCaseOptions);


    private static string SerializeToJson<T>(T payload) =>
        JsonSerializer.Serialize(payload, CamelCaseOptions);


    //private static string SerializeToJson<T>(T payload)
    //{
    //    return JsonSerializer.Serialize(payload, new JsonSerializerOptions
    //    {
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    //    });
    //}

}
