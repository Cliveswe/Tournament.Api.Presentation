// Ignore Spelling: json Deserializes deserialize deserialization
using System.Net.Http.Headers;
using System.Text.Json;

namespace Tournaments.Client.Clients;

public class TournamentsClient : ITournamentsClient
{

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
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.Json));
        client.Timeout = new TimeSpan(0, 0, 5);//Timeout in 5 seconds.
    }

    public async Task<T> GetAsync<T>(string path, string contentType = MediaTypes.Json)
    {
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
        HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        string stream = await response.Content.ReadAsStringAsync();
        T? result = DeserializeFromJson<T>(stream);
        return result!;
    }

    public async Task<TResult?> SendAsync<TCreate, TResult>(
    HttpMethod httpMethod,
    string path,
    TCreate payload,
    string contentType = MediaTypes.Json)
    {
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        // Serialize payload with correct serialize method
        string serializedPayload;
        if (IsJsonPatchDocument(payload))
        {
            serializedPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
        }
        else
        {
            serializedPayload = JsonSerializer.Serialize(payload, CamelCaseOptions);
        }

        // Create request
        using HttpRequestMessage request = new HttpRequestMessage(httpMethod, path)
        {
            Content = new StringContent(serializedPayload)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using HttpResponseMessage response = await client.SendAsync(request);

        try
        {
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();

            // For void responses (e.g. Patch returning no content), handle accordingly
            if (typeof(TResult) == typeof(object) || string.IsNullOrWhiteSpace(result))
            {
                return default;
            }

            return JsonSerializer.Deserialize<TResult>(result, CamelCaseOptions);
        }
        catch
        {
            // Optionally log or handle exceptions here
            return default;
        }
    }

    private static bool IsJsonPatchDocument(object payload)
    {
        var type = payload.GetType();
        if (!type.IsGenericType)
            return false;
        return type.GetGenericTypeDefinition() == typeof(Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<>);
    }



    /*
     * Examples of using SendAsync
     * await SendAsync(HttpMethod.Post, "api/path", () => new MyCreateDto());
     * await SendAsync(HttpMethod.Put, "api/path/123", () => new MyUpdateDto());
     * await SendAsync(new HttpMethod("PATCH"), "api/path/123", () => new MyPatchDto());
     */

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


    private static string SerializeToJson<T>(T payload)
    {
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));

        if (payload is Microsoft.AspNetCore.JsonPatch.JsonPatchDocument)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(
                payload,
                new Newtonsoft.Json.JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                    {
                        NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()
                    }
                });
        }
        else
        {
            return JsonSerializer.Serialize(payload, CamelCaseOptions);
        }
    }




    //private static string SerializeToJson<T>(T payload)
    //{
    //    return JsonSerializer.Serialize(payload, new JsonSerializerOptions
    //    {
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    //    });
    //}

}
