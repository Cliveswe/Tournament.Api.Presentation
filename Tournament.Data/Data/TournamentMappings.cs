using AutoMapper;
using Tournament.Core.Dto;
using Tournament.Core.Entities;

namespace Tournament.Data.Data;
public class TournamentMappings : Profile
{
    public TournamentMappings()
    {
        // Create mappings between DTOs and entities.
        _ = CreateMap<TournamentDetails, TournamentDto>()
            .ForMember(dest => dest.Games, opt => opt.MapFrom(src => src.Games));

        // Map TournamentUpdateDto to TournamentDetails.
        // Ignore StartDate and Id as they are not part of the update DTO
        // and ignore Games as they are not part of the update DTO.
        // This allows you to update only the Title of the tournament without affecting other properties.
        _ = CreateMap<TournamentUpdateDto, TournamentDetails>()
            .ForMember(dest => dest.StartDate, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        _ = CreateMap<TournamentDetailsCreateDto, TournamentDetails>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        _ = CreateMap<GameCreateDto, Game>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name.Trim())); // Trim whitespace from Title

        CreateMap<Game, GameDto>();


        //Show users a more descriptive name like StartDate instead of the raw database property name Time.
        //Mapping lets you decouple your database model from the API contract.
        _ = CreateMap<Game, GameDto>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Time)); // adjust if needed;

        // Map GameUpdateDto to Game
        // Ignore Id to allow updates without changing it. Ignore Time if not needed in update.
        // Ignore TournamentId if not needed in update. Ignore navigation property to avoid circular references.
        // This allows you to update only the Title of the game without affecting other properties.
        _ = CreateMap<GameUpdateDto, Game>()
             .ForMember(dest => dest.Id, opt => opt.Ignore()) // Ignore Id to allow updates without changing it
             .ForMember(dest => dest.Time, opt => opt.Ignore()) // Ignore Time if not needed in update
             .ForMember(dest => dest.TournamentDetailsId, opt => opt.Ignore()) // Ignore TournamentId if not needed in update
             .ForMember(dest => dest.TournamentDetails, opt => opt.Ignore()); // Ignore navigation property to avoid circular references
    }
}
