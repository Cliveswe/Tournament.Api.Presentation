using AutoMapper;
using Tournament.Core.Dto;
using Tournament.Core.Entities;

namespace Tournament.Data.Data;
public class TournamentMappings : Profile
{
    public TournamentMappings()
    {
        CreateMap<TournamentDetails, TournamentDto>();

        //Show users a more descriptive name like StartDate instead of the raw database property name Time.
        //Mapping lets you decouple your database model from the API contract.
        CreateMap<Game, GameDto>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Time)); // adjust if needed;


    }
}
