// Ignore Spelling: json
using System.Net.Http.Headers;

namespace Tournaments.Client.Clients;

public class TournamentsClient
{
    private const string json = "application/json";
    private readonly HttpClient client;

    public TournamentsClient(HttpClient client)
    {
        this.client = client;
        this.client.BaseAddress = new Uri("https://localhost:7225");
        this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(json));
        this.client.Timeout = new TimeSpan(0, 0, 5);//Timeout in 5 seconds.
    }


}
