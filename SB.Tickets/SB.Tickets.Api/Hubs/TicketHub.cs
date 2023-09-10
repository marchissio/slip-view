using Microsoft.AspNetCore.SignalR;
using SB.Tickets.Api.Services;

namespace SB.Tickets.Api.Hubs
{
    public class TicketHub : Hub<ITicketHub>
    {
        private readonly ILogger<TicketHub> _logger;
        private readonly ITicketService _ticketService;

        public TicketHub(ILogger<TicketHub> logger, ITicketService ticketService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        }

        public Task SubscribeOnTickets(string group)
        {
            _logger.LogDebug($"SubscribeOnTickets, ConnectionId: {Context.ConnectionId}, group {group}");
            return _ticketService.SubscribeOnTickets(Context.ConnectionId, group);
        }

        public Task UnsubscribeFromTickets(string group)
        {
            _logger.LogDebug($"UnsubscribeFromTickets, ConnectionId: {Context.ConnectionId}, group {group}");
            return _ticketService.UnsubscribeFromTickets(Context.ConnectionId, group);
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogDebug($"OnConnectedAsync, ConnectionId: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogDebug($"OnDisconnectedAsync, ConnectionId: {Context.ConnectionId}, Exception: {exception?.Message}");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
