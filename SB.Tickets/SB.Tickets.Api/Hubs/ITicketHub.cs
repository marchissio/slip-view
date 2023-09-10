
namespace SB.Tickets.Api.Hubs
{
    public interface ITicketHub
    {
        Task TicketChange(string ticket);
        Task AllTicketsForType(string tickets);
    }
}
