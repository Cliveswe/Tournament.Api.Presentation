// Ignore Spelling: json
namespace Tournaments.Client.Clients;
public interface ITournamentsClient
{

    Task<T> GetAsync<T>(string path, string contentType = MediaTypes.Json);

    Task<TResult?> SendAsync<TCreate, TResult>(
      HttpMethod httpMethod,
      string path,
      TCreate payload,
      string contentType = MediaTypes.Json);
}