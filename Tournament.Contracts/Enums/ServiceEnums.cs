//add to ignore Enums

// --------------------------------------------------------------------------------
// File: ServiceEnums.cs
// Summary: Defines enums representing the possible outcomes of game-related
//          service operations, such as updating and patching games. These enums
//          facilitate clear communication of operation results between the service
//          layer and controllers, enhancing code readability and maintainability.
// <author> [Clive Leddy] </author>
// <created> [2025-07-11] </created>
// Notes: Used to standardize responses for update and patch operations, aiding in
//        precise API response handling and error reporting.
// --------------------------------------------------------------------------------

namespace Service.Contracts.Enums
{
    /// <summary>
    /// Represents possible results returned by the UpdateAsync method for updating a game.
    /// </summary>
    public enum UpdateGameResult
    {
        /// <summary>
        /// Indicates the game to update was not found.
        /// </summary>
        NotFound,

        /// <summary>
        /// Indicates no changes were detected or saved during the update.
        /// </summary>
        NotModified,

        /// <summary>
        /// Indicates the update was successful.
        /// </summary>
        Success
    }

    /// <summary>
    /// Represents possible outcomes of applying a JSON Patch document to a game entity.
    /// </summary>
    public enum ApplyPatchResult
    {
        /// <summary>
        /// Indicates the game to patch was not found.
        /// </summary>
        GameNotFound,

        /// <summary>
        /// Indicates the tournament associated with the game was not found.
        /// </summary>
        TournamentNotFound,

        /// <summary>
        /// Indicates the game date is not within the valid tournament period.
        /// </summary>
        InvalidDateRange,

        /// <summary>
        /// Indicates no changes were made or saved after applying the patch.
        /// </summary>
        NoChanges,

        /// <summary>
        /// Indicates the patch was successfully applied and persisted.
        /// </summary>
        Success
    }
}
