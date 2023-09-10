
namespace SB.Tickets.Api.State
{
    public class Match
    {
        public long Id { get; }
        public int? Status { get; set; }
        public string Home { get; set; }
        public string Away { get; set; }
        public int Phase { get; set; }
        public int LeagueId { get; set; }
        public string League { get; set; }
        public string Sport { get; set; }
        public int Round { get; set; }
        public DateTime? KickoffTime { get; set; }

        public MatchLive Live { get; set; }

        public Match(long id)
        {
            Id = id;
        }
    }
}
