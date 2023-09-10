
namespace SB.Tickets.Import.Models
{
    /// <summary>
    /// Has complex primary key: TICKET (UUID),BOOK,ROUND,MATCH_CODE,TYPE_CODE
    /// </summary>
    public class MatchType
    {
        public int MATCH_CODE { get; set; } //Match code within the round
        public int TYPE_CODE { get; set; } //od_bet_pick code
        public string LIVE { get; set; } //is Live Y/N
        public int? TICKET_SYSTEM { get; set; } //System id - npr. 1
        public decimal? ODD_VALUE { get; set; } //Odd value
        public int? ODD_STATUS { get; set; } //LOSS(-1), UNKNOWN(0), WIN(1), NO_BET(2) (odd value=1)
        public string SPECIAL_VALUE { get; set; } //Handicap, Over/Under, other special value for type
        public int? VOID_REASON { get; set; } //Reason because odd for type is 1 (OddStatus = 2)
        public long MATCH_ID { get; set; }
    }
}
