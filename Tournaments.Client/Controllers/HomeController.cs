// Ignore Spelling: api json Deserialize deserialization Deserializes

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using Tournaments.Client.Clients;
using Tournaments.Shared.Dto;

namespace Tournaments.Client.Controllers;
public class HomeController : Controller
{
    private const string json = "application/json";
    private const string jsonPatch = "application/json-patch+json";//NB, this is only used calling a PATCH endpoint.
    private readonly ITournamentsClient tournamentsClient;
    private HttpClient httpClient;

    public HomeController(IHttpClientFactory httpClientFactory, ITournamentsClient tournamentsClient)
    {
        //httpClient = httpClientFactory.CreateClient();//creates a single instance from the client factory.
        //httpClient.BaseAddress = new Uri("https://localhost:7225");

        httpClient = httpClientFactory.CreateClient("TournamentsClient");
        this.tournamentsClient = tournamentsClient;
    }

    public async Task<IActionResult> Index()
    {
        IEnumerable<TournamentDto>? result = await SimpleGetAsync();
        IEnumerable<TournamentDto>? result2 = await SimpleGetAsync2<IEnumerable<TournamentDto>>();
        IEnumerable<TournamentDto>? result3 = await GetWithRequestMessage(HttpMethod.Get, "api/tournamentDetails");
        TournamentDto? result4 = await PostWithRequestMessageAsync(HttpMethod.Post, "api/tournamentDetails");
        await PatchWithReqestMessageAsync(HttpMethod.Patch, "api/tournamentDetails/1/games/51");
        return View();
    }

    private async Task PatchWithReqestMessageAsync(HttpMethod httpMethod, string target)
    {
        JsonPatchDocument<GameUpdateDto> patchDocument = new JsonPatchDocument<GameUpdateDto>();

        patchDocument.Replace(g => g.Title, "Test patch from \"homecontroller\"");

        string serializedPatchDoc = Newtonsoft.Json.JsonConvert.SerializeObject(patchDocument);

        HttpRequestMessage request = new HttpRequestMessage(httpMethod, target);

        // Accept JSON response
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(json));

        // Set correct content with proper Content-Type for JSON Patch
        request.Content = new StringContent(serializedPatchDoc);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(jsonPatch);

        HttpResponseMessage response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

    }

    private async Task<IEnumerable<TournamentDto>> SimpleGetAsync()
    {
        var test = httpClient;
        HttpResponseMessage response = await httpClient.GetAsync("api/tournamentDetails");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();//returns a string.

        //Deserialize the string to json.
        IEnumerable<TournamentDto>? tournaments = DeserializeApiResponse<IEnumerable<TournamentDto>>(result);

        return tournaments!;

    }

    private async Task<T?> SimpleGetAsync2<T>() => await httpClient.GetFromJsonAsync<T>("api/tournamentDetails",
           new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    private async Task<IEnumerable<TournamentDto>?> GetWithRequestMessage(HttpMethod httpMethod, string target)
    {
        //HttpRequestMessage request = HttpHomeControllerRequestMessage(httpMethod, target);
        //HttpResponseMessage response = await httpClient.SendAsync(request);
        //response.EnsureSuccessStatusCode();
        //string result = await response.Content.ReadAsStringAsync();
        //IEnumerable<TournamentDto>? tournaments = DeserializeApiResponse<IEnumerable<TournamentDto>>(result);
        //return tournaments!;
        return await tournamentsClient.GetAsync<IEnumerable<TournamentDto>>(target);
    }

    private async Task<TournamentDto?> PostWithRequestMessageAsync(HttpMethod httpMethod, string target)
    {
        HttpRequestMessage request = HttpHomeControllerRequestMessage(httpMethod, target);
        TournamentDetailsCreateDto tournamentToCreate = new TournamentDetailsCreateDto
        {
            Title = "ABC httpClient",
            StartDate = DateTime.Now
        };
        string jsonTournament = JsonSerializer.Serialize(tournamentToCreate);
        request.Content = new StringContent(jsonTournament);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(json);
        HttpResponseMessage response = await httpClient.SendAsync(request);

        try {
            //Throws an exception if the HTTP response is false.
            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();
            TournamentDto? tournamentDto = DeserializeApiResponse<TournamentDto>(result);
            Uri? location = response.Headers.Location;//get the uri for the newly created tournament.
            return tournamentDto;

        } catch(Exception ex) {
            _ = ex;//keep the compiler quite!!
            return null;
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
    private static TResult? DeserializeApiResponse<TResult>(string result)
    {
        return JsonSerializer
            .Deserialize<TResult>(result,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
    }

    /// <summary>
    /// Creates an <see cref="HttpRequestMessage"/> with the specified HTTP method and target URI, 
    /// and sets the "Accept" header to "application/json".
    /// </summary>
    /// <param name="httpMethod">The HTTP method to use (e.g., GET, POST).</param>
    /// <param name="target">The target URI for the HTTP request.</param>
    /// <returns>
    /// A configured <see cref="HttpRequestMessage"/> ready to be sent using <see cref="HttpClient"/>.
    /// </returns>
    private static HttpRequestMessage HttpHomeControllerRequestMessage(HttpMethod httpMethod, string target)
    {
        HttpRequestMessage request = new HttpRequestMessage(httpMethod,target);
        request
           .Headers
           .Accept
           .Add(new MediaTypeWithQualityHeaderValue(json));
        return request;
    }
}
