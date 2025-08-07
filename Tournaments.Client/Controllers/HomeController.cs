// Ignore Spelling: api json Deserialize deserialization

using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using Tournaments.Shared.Dto;

namespace Tournaments.Client.Controllers;
public class HomeController : Controller
{
    private const string json = "application/json";
    private HttpClient httpClient;

    public HomeController()
    {
        httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7225");
    }

    public async Task<IActionResult> Index()
    {
        IEnumerable<TournamentDto> result = await SimpleGetAsync();
        IEnumerable<TournamentDto> result2 = await SimpleGetAsync2();
        IEnumerable<TournamentDto> result3 = await GetWithRequestMessage(HttpMethod.Get, "api/tournamentDetails");
        return View();
    }

    private async Task<IEnumerable<TournamentDto>> SimpleGetAsync()
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/tournamentDetails");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();//returns a string.

        //Deserialize the string to json.
        IEnumerable<TournamentDto>? tournaments = DeserializeApiResponse(result);

        return tournaments!;
    }

    /// <summary>
    /// Deserialize the string to json and make sure that we get Camel Case for a C# object.
    /// Note: the Api returns an ApiBaseResponse thus in this project the
    /// "PropertyNamingPolicy = JsonNamingPolicy.CamelCase" is not needed. However, add the 
    /// policy to guard the deserialization result.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private static IEnumerable<TournamentDto>? DeserializeApiResponse(string result)
    {
        return JsonSerializer
            .Deserialize<IEnumerable<TournamentDto>>(result,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
    }

    private async Task<IEnumerable<TournamentDto>> SimpleGetAsync2() => await httpClient.GetFromJsonAsync<IEnumerable<TournamentDto>>("api/tournamentDetails",
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });


    private async Task<IEnumerable<TournamentDto>> GetWithRequestMessage(HttpMethod httpMethod, string target)
    {

        //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/Tournaments");
        HttpRequestMessage request = new HttpRequestMessage(httpMethod, target);
        request
            .Headers
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue(json));
        HttpResponseMessage response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        string result = await response.Content.ReadAsStringAsync();
        IEnumerable<TournamentDto>? tournaments = DeserializeApiResponse(result);
        return tournaments!;
    }
}
