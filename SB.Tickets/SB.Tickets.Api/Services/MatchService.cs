using SB.Tickets.Api.State;

namespace SB.Tickets.Api.Services
{
    public interface IMatchService
    {
        Match GetMatch(long id);
    }
    
    public class MatchService : IMatchService
    {
        private readonly ILogger<MatchService> _logger;
        private readonly Dictionary<long, Match> _matchCache = new Dictionary<long, Match> 
        {
            { 475856912, new Match(475856912) { Home = "Real Madrid", Away = "Barselona", KickoffTime = DateTime.UtcNow.AddHours(3),
                LeagueId = 1, League = "LS", Sport = "S" 
            } },
            { 475858258, new Match(475858258) { Home = "Everton", Away = "Chelsea", KickoffTime = DateTime.UtcNow.AddHours(4),
                LeagueId = 2, League = "Engleska 1", Sport = "S"
            } }
        };

        public MatchService(ILogger<MatchService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Match GetMatch(long id)
        {
            try
            {
                if (_matchCache.TryGetValue(id, out Match match))
                {
                    return CloneMatch(match);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(GetMatch));
            }
            return null;
        }

        private Match CloneMatch(Match match)
        {
            return new Match(match.Id)
            {
                Status = match.Status,
                Home = match.Home,
                Away = match.Away,
                Phase = match.Phase,
                LeagueId = match.LeagueId,
                League = match.League,
                Sport = match.Sport,
                Round = match.Round,
                KickoffTime = match.KickoffTime,
            };
        }
    }
}
