// -------------------------------------------------------------------------------
// File: TournamentMappings.cs
// Summary: Defines AutoMapper profile for mapping between domain entities and
//          DTOs within the Tournament API. Handles transformations for tournament
//          and game data models across create, update, and read operations.
// Author: [Clive Leddy]
// Created: [2025-06-30]
// Notes: Inherits from AutoMapper.Profile to register mapping configurations.
//        Includes trimming, property ignoring, and renaming logic to decouple 
//        persistence layer models from API-facing DTOs.
// -------------------------------------------------------------------------------


using AutoMapper;
using Tournament.Core.Dto;
using Tournament.Core.Entities;

namespace Tournament.Data.Data;

/// <summary>
/// Provides AutoMapper configuration for mapping between domain entities and data transfer objects (DTOs)
/// used in the Tournament API. 
/// </summary>
/// <remarks>
/// This profile defines explicit mapping rules to transform between the following object pairs:
/// <list type="bullet">
/// <item><see cref="TournamentDetails"/> ↔ <see cref="TournamentDto"/></item>
/// <item><see cref="TournamentUpdateDto"/> → <see cref="TournamentDetails"/> (ignoring StartDate, Id, and Games)</item>
/// <item><see cref="TournamentDetailsCreateDto"/> → <see cref="TournamentDetails"/> (ignoring Id)</item>
/// <item><see cref="GameCreateDto"/> → <see cref="Game"/> (ignoring Id and trimming Name to Title)</item>
/// <item><see cref="Game"/> → <see cref="GameDto"/> (mapping Time to StartDate)</item>
/// <item><see cref="GameUpdateDto"/> → <see cref="Game"/> (ignoring Id, Time, TournamentDetailsId, and navigation properties)</item>
/// </list>
/// This configuration ensures a clean separation between API contracts and persistence models,
/// supporting safer updates, avoiding over-posting, and improving API clarity.
/// </remarks>

public class TournamentMappings : Profile
{
    public TournamentMappings()
    {
        /// <summary>
        /// Maps between <see cref="TournamentDetails"/> and <see cref="TournamentDto"/>, including nested game lists.
        /// Enables full bidirectional mapping with <c>ReverseMap</c>.
        /// </summary>
        _ = CreateMap<TournamentDetails, TournamentDto>()
            .ForMember(dest => dest.Games, opt => opt.MapFrom(src => src.Games))
            .ReverseMap();

        /// <summary>
        /// Maps from <see cref="TournamentUpdateDto"/> to <see cref="TournamentDetails"/>, ignoring:
        /// <list type="bullet">
        /// <item><c>StartDate</c> — as it's immutable in updates</item>
        /// <item><c>Id</c> — to avoid ID manipulation</item>
        /// <item><c>Games</c> — to prevent altering game associations during a tournament update</item>
        /// </list>
        /// <remarks>
        /// Ignore StartDate and Id as they are not part of the update DTO and ignore Games as they are 
        /// not part of the update DTO. This allows you to update only the Title of the tournament without 
        /// affecting other properties.
        /// </remarks>
        /// </summary>
        _ = CreateMap<TournamentUpdateDto, TournamentDetails>()
            .ForMember(dest => dest.StartDate, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        /// <summary>
        /// Maps from <see cref="TournamentDetailsCreateDto"/> to <see cref="TournamentDetails"/>,
        /// ignoring the <c>Id</c> to let the database assign it.
        /// </summary>
        _ = CreateMap<TournamentDetailsCreateDto, TournamentDetails>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        /// <summary>
        /// Maps from <see cref="GameCreateDto"/> to <see cref="Game"/>, ignoring the <c>Id</c>,
        /// and trimming the <c>Name</c> before mapping it to <c>Title</c>.
        /// </summary>
        _ = CreateMap<GameCreateDto, Game>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name.Trim())); // Trim whitespace from Title

        _ = CreateMap<Game, GameDto>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Time))
            .ReverseMap()
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.StartDate));


        /// <summary>
        /// Maps from <see cref="GameUpdateDto"/> to <see cref="Game"/>, ignoring:
        /// <list type="bullet">
        /// <item><c>Id</c> — prevents changes to the entity’s key</item>
        /// <item><c>Time</c>, <c>TournamentDetailsId</c>, <c>TournamentDetails</c> — to avoid accidental updates or circular references</item>
        /// </list>
        /// <remarks>
        /// Ignore Id to allow updates without changing it. Ignore Time if not needed in update.
        /// Ignore TournamentId if not needed in update. Ignore navigation property to avoid circular references.
        /// This allows you to update only the Title of the game without affecting other properties.
        /// </remarks>
        /// </summary>
        _ = CreateMap<GameUpdateDto, Game>()
             .ForMember(dest => dest.Id, opt => opt.Ignore()) // Ignore Id to allow updates without changing it
             .ForMember(dest => dest.Time, opt => opt.Ignore()) // Ignore Time if not needed in update
             .ForMember(dest => dest.TournamentDetailsId, opt => opt.Ignore()) // Ignore TournamentId if not needed in update
             .ForMember(dest => dest.TournamentDetails, opt => opt.Ignore()); // Ignore navigation property to avoid circular references
    }
}
