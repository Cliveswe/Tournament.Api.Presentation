using Domain.Models.Responses;
using Tournaments.Shared.Dto;
using Tournaments.Shared.Request;

namespace Service.Contracts;

public interface ITournamentService
{

    Task<(ApiBaseResponse tournamentDto, MetaData metaData)> GetAllAsync(TournamentRequestParameters requestParameters, bool trackChanges = false);
    Task<ApiBaseResponse> GetByIdAsync(int id, bool trackChanges = false);
    Task<(int id, TournamentDto tournamentDto)> CreateAsync(TournamentDetailsCreateDto tournamentDetailsCreateDto);

    Task<bool> ApplyToAsync(int id, TournamentDto tournamentUpdateDto);

    Task<bool> Update(int id, TournamentUpdateDto tournamentUpdateDto);
    Task<ApiBaseResponse> ExistsAsync(string title, DateTime startDate);
    Task<ApiBaseResponse> ExistsAsync(int id);
    Task<bool> RemoveAsync(int id);
}