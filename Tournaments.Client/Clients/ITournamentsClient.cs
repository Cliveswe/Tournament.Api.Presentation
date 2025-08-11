// Ignore Spelling: json
namespace Tournaments.Client.Clients;
public interface ITournamentsClient
{

    Task<T> GetAsync<T>(string path, string contentType = MediaTypes.Json);

    Task<TResult?> SendAsync<TCreate, TResult>(
        HttpMethod httpMethod,
        string path,
        Func<TCreate> payloadFactory,
        string contentType = MediaTypes.Json);
}