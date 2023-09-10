
namespace SB.Tickets.Api.Models
{
	public class TicketView
	{
		public string Id { get; set; }
		public string Code { get; set; }
		public int Type { get; set; }
		public int Source { get; set; }
		public bool IsFavourite { get; set; }
		public string User { get; set; }
		public string UserName { get; set; }
		public string UserNick { get; set; }
		public string Location { get; set; }
		public int Status { get; set; }
		public int PStatus { get; set; }
		public DateTime? PayinTime { get; set; }
		public decimal Payin { get; set; }
		public string Currency { get; set; }
		public decimal MinTotalOdd { get; set; }
		public decimal MaxTotalOdd { get; set; }
		public string TicketStructure {get;set;}
		public List<TicketLineView> TicketLines { get; set; }
	}
}