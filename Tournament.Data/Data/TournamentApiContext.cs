// Ignore Spelling: Api

using Microsoft.EntityFrameworkCore;

namespace Tournament.Data.Data
{
    public class TournamentApiContext : DbContext
    {
        public TournamentApiContext(DbContextOptions<TournamentApiContext> options)
            : base(options)
        {
        }

        public DbSet<Core.Entities.TournamentDetails> TournamentDetails { get; set; } = default!;
        public DbSet<Core.Entities.Game> Game { get; set; } = default!;
    }
}
