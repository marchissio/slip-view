
namespace SB.Tickets.Api.State
{
    public class TicketLine
    {
        public int TypeCode { get; set; }
        public bool IsLive { get; set; }
        public string SpecialValue { get; set; }
        public int? TicketSystem { get; set; }
        public decimal? OddValue { get; set; }
        public int? OddStatus { get; set; }
        public long MatchId { get; set; }
        public int MatchCode { get; set; }
    }
}
