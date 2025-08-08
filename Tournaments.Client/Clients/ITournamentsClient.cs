// Ignore Spelling: json
namespace Tournaments.Client.Clients;
public interface ITournamentsClient
{
    Task<T> GetAsync<T>(string path, string contentType = "application/json");
}