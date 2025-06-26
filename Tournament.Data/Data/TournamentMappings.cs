using AutoMapper;
using Tournament.Core.Dto;
using Tournament.Core.Entities;

namespace Tournament.Data.Data;
public class TournamentMappings : Profile
{
    public TournamentMappings()
    {
        // Map TournamentDetails to TournamentDto
        CreateMap<TournamentDetails, TournamentDto>();

        // Map TournamentUpdateDto to TournamentDetails
        // Ignore StartDate and Id as they are not part of the update DTO
        // and ignore Games as they are not part of the update DTO
        // This allows you to update only the Title of the tournament without affecting other properties.
        CreateMap<TournamentUpdateDto, TournamentDetails>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.StartDate, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Games, opt => opt.Ignore());

        //Show users a more descriptive name like StartDate instead of the raw database property name Time.
        //Mapping lets you decouple your database model from the API contract.
        CreateMap<Game, GameDto>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Time)); // adjust if needed;

    }
}
