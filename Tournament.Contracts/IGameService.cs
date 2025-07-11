using Service.Contracts.Enums;
using Tournaments.Shared.Dto;

namespace Service.Contracts;

public interface IGameService
{
    Task<IEnumerable<GameDto>> GetAllAsync(int tournamentId);
    Task<GameDto?> GetAsync(int tournamentId, int id);
    Task<GameDto> GetAsync(int tournamentId, string title);
    Task<(bool isSuccess, bool isDuplicate, GameDto? gameDto)> AddAsync(GameCreateDto gameCreateDto, int tournamentId);
    Task<UpdateGameResult> UpdateAsync(int tournamentId, string title, GameUpdateDto gameUpdateDto);
    Task<ApplyPatchResult> ApplyToAsync(int tournamentId, int id, GameDto gameDto, TournamentDto tournamentDto);
    Task<bool> RemoveAsync(int tournamentId, int id);
    Task<bool> ExistsAsync(int id);
}