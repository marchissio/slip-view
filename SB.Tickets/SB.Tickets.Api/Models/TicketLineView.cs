
namespace SB.Tickets.Api.Models
{
    public class TicketLineView
    {
        public int MatchCode { get; set; }
        public string BetTypeName { get; set; }
        public string BetTypeShortName { get; set; }
        public string SpecialValue { get; set; }
        public bool IsLive { get; set; }
        public int? TicketSystem { get; set; }
        public decimal? OddValue { get; set; }
        public int? OddStatus { get; set; }

        public string Sport { get; set; }
        public string League { get; set; }
        public string Home { get; set; }
        public string Away { get; set; }
        public int? Status { get; set; }
        public int? Phase { get; set; }
        public int? Round { get; set; }
        public DateTime? KickoffTime { get; set; }

        public int? StatusInLive { get; set; }
        public int? PhaseInLive { get; set; }
    }
}