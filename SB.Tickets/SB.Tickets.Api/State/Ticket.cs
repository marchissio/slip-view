namespace SB.Tickets.Api.State
{
    public class Ticket
    {
        public string Uuid { get; set; }
        public string Code { get; set; }
        public string Instance { get; set; }
        public TicketType Type { get; set; }
        public TicketSource Source { get; set; }
        public bool IsFavourite { get; set; }
        public string UserUuid { get; set; }
        public int LocationCode { get; set; }
        public int Status { get; set; }
        public int PStatus { get; set; }
        public DateTime CTime { get; set; }
        public DateTime? PayinTime { get; set; }
        public decimal Payin { get; set; }
        public string Currency { get; set; }
        public decimal MinTotalOdd { get; set; }
        public decimal MaxTotalOdd { get; set; }
        public string TicketStructure {get;set;}
        public List<TicketLine> TicketLines { get; set; }
    }
}
