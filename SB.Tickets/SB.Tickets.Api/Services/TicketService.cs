using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SB.Tickets.Import.Models;
using SB.Tickets.Api.Config;
using SB.Tickets.Api.Hubs;
using SB.Tickets.Api.State;
using System.Collections.Concurrent;

namespace SB.Tickets.Api.Services
{
    public interface ITicketService
    {
        Task SubscribeOnTickets(string connectionId, string group);
        Task UnsubscribeFromTickets(string connectionId, string group);
        void TicketChange(W3Ticket w3Ticket);
    }

    public class TicketService : ITicketService
    {
        private readonly ILogger<TicketService> _logger;
        private readonly ImportConfig _importConfig;
        private readonly IUserService _userService;
        private readonly ILocationService _locationService;
        private readonly ITipTypeService _tipTypeService;
        private readonly IMatchService _matchService;
        private readonly IHubContext<TicketHub, ITicketHub> _ticketHub;
        private readonly ConcurrentDictionary<string, Dictionary<string, Ticket>> _ticketsCache = new();

        private readonly int ADD_TICKET_INTERVAL =  1000 * 5;
        private readonly Timer _addTicketTimer;

        public TicketService(
            ILogger<TicketService> logger,
            IOptions<ImportConfig> importConfig,
            IUserService userService,
            ILocationService locationService,
            ITipTypeService tipTypeService,
            IMatchService matchService,
            IHubContext<TicketHub, ITicketHub> ticketHub)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _importConfig = importConfig.Value ?? throw new ArgumentNullException(nameof(importConfig));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            _tipTypeService = tipTypeService ?? throw new ArgumentNullException(nameof(tipTypeService));
            _matchService = matchService ?? throw new ArgumentNullException(nameof(matchService));
            _ticketHub = ticketHub ?? throw new ArgumentNullException(nameof(ticketHub));
            _addTicketTimer = new Timer(AddTicketTimerCallback, null, ADD_TICKET_INTERVAL, Timeout.Infinite);
        }

        private void AddTicketTimerCallback(Object state)
        {
            var ticket = JsonConvert.DeserializeObject<W3Ticket>(_ticketTemplate);
            ticket.UUID = Guid.NewGuid().ToString();
            TicketChange(ticket);
            _addTicketTimer.Change(ADD_TICKET_INTERVAL, Timeout.Infinite);
        }

        public async Task SubscribeOnTickets(string connectionId, string group)
        {
            try
            {
                await _ticketHub.Groups.AddToGroupAsync(connectionId, group);
                if (_ticketsCache.TryGetValue(group, out Dictionary<string, Ticket> tickets))
                {
                    List<Models.TicketView> ticketsView = tickets.OrderByDescending(t => t.Value.PayinTime).Select(t => TicketToTicketView(t.Value)).ToList();
                    string message = JsonConvert.SerializeObject(ticketsView);
                    await _ticketHub.Clients.Client(connectionId).AllTicketsForType(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("SubscribeOnTickets", ex);
                throw;
            }
        }

        public async Task UnsubscribeFromTickets(string connectionId, string group)
        {
            await _ticketHub.Groups.RemoveFromGroupAsync(connectionId, group);
        }

        public void TicketChange(W3Ticket w3Ticket)
        {
            try
            {
                _logger.LogDebug("Start processing ticket");
                TicketType ticketType = MapTicketType(w3Ticket);
                if (ticketType == TicketType.Unknown || string.IsNullOrEmpty(w3Ticket.INSTANCE_CODE))
                {
                    _logger.LogError($"Unknown ticket: type {w3Ticket.TICKET_TYPE}, instance {w3Ticket.INSTANCE_CODE}");
                    return;
                }
                string group = GetGroupName(w3Ticket.INSTANCE_CODE, ticketType);
                if (!_ticketsCache.TryGetValue(group, out Dictionary<string, Ticket> tickets))
                {
                    tickets = new Dictionary<string, Ticket>();
                    _ticketsCache.TryAdd(group, tickets);
                }
                
                if (tickets.TryGetValue(w3Ticket.UUID, out Ticket ticket))
                {
                    bool changed = false;
                    if (ticket.Status != w3Ticket.STATUS)
                    {
                        ticket.Status = w3Ticket.STATUS;
                        changed = true;
                    }
                    if (ticket.PStatus != w3Ticket.PSTATUS)
                    {
                        ticket.PStatus = w3Ticket.PSTATUS;
                        changed = true;
                    }
                    if (w3Ticket.MATCH_TYPES != null)
                    {
                        foreach (var mt in w3Ticket.MATCH_TYPES)
                        {
                            var ticketLine = ticket.TicketLines.FirstOrDefault(x => x.MatchCode == mt.MATCH_CODE && x.TypeCode == mt.TYPE_CODE);
                            if (ticketLine != null)
                            {
                                ticketLine.OddStatus = mt.ODD_STATUS;
                            }
                            else
                                _logger.LogWarning($"Not found ticket line for update, TicketUuid: {ticket.Uuid}, line MatchCode: {mt.MATCH_CODE}, line TypeCode {mt.TYPE_CODE}");
                        }
                    }

                    if (changed)
                    {
                        NotifyTicketChange(group, ticket);
                    }
                }
                else
                {
                    var ticketNew = W3TicketToTicket(ticketType, w3Ticket);
                    tickets.TryAdd(ticketNew.Uuid, ticketNew);

                    //Ako broj tiketa predje na primer 110, brisemo najstarijih 10, da ne bismo brisali svaki put
                    if (tickets.Count >= 110)
                    {
                        List<string> toBeRemoved = tickets.OrderBy(x => x.Value.CTime).Select(x => x.Key).Take(10).ToList();
                        _logger.LogInformation($"Removing tickets from cache count: {toBeRemoved.Count}, {JsonConvert.SerializeObject(toBeRemoved)}");
                        foreach (string uuid in toBeRemoved)
                            tickets.Remove(uuid);
                    }

                    NotifyTicketChange(group, ticketNew);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(TicketChange));
                throw new Exception($"Error while processing W3Ticket UUID: {w3Ticket.UUID}, ID: {w3Ticket.ID}");
            }
        }

        private string GetGroupName(string instance, TicketType ticketType)
        {
            return $"{instance}-{(int)ticketType}";
        }

        private void NotifyTicketChange(string group, Ticket ticket)
        {
            try
            {
                Models.TicketView ticketView = TicketToTicketView(ticket);
                string message = JsonConvert.SerializeObject(ticketView);
                _ticketHub.Clients.Group(group)?.TicketChange(message).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"NotifyTicketChange: {ex.Message}");
            }
        }

        private TicketType MapTicketType(W3Ticket w3Ticket)
        {
            if (w3Ticket.TICKET_TYPE == "B" || w3Ticket.TICKET_TYPE == "I" || w3Ticket.TICKET_TYPE == "K")
                return TicketType.Prematch;
            else if (w3Ticket.TICKET_TYPE == "N")
                return TicketType.Live;
            else if (w3Ticket.TICKET_TYPE == "J" || w3Ticket.TICKET_TYPE == "L")
            {
                if (w3Ticket.MATCH_TYPES.Any(mt => mt.LIVE != "Y"))
                    return TicketType.Mix;
                else
                    return TicketType.Live;
            }
            else
                return TicketType.Unknown;
        }

        private TicketSource MapTicketSource(W3Ticket w3Ticket)
        {
            if (w3Ticket.TICKET_TYPE == "B" || w3Ticket.TICKET_TYPE == "L" || w3Ticket.TICKET_TYPE == "K" || w3Ticket.TICKET_TYPE == "N")
                return TicketSource.Retail;
            else if (w3Ticket.TICKET_TYPE == "I" || w3Ticket.TICKET_TYPE == "J")
                return TicketSource.Web;
            else
                return TicketSource.Unknown;
        }

        private Ticket W3TicketToTicket(TicketType ticketType, W3Ticket w3Ticket)
        {
            Ticket ticketNew = new()
            {
                Uuid = w3Ticket.UUID,
                Instance = w3Ticket.INSTANCE_CODE,
                Type = ticketType,
                Source = MapTicketSource(w3Ticket),
                Code = w3Ticket.CODE,
                IsFavourite = w3Ticket.TERMINAL == _importConfig.FavouriteTerminal,
                UserUuid = w3Ticket.USER_UUID,
                LocationCode = w3Ticket.LOCATION,
                Status = w3Ticket.STATUS,
                PStatus = w3Ticket.PSTATUS,
                CTime = w3Ticket.CTIME,
                PayinTime = w3Ticket.PAYIN_TIME,
                Payin = w3Ticket.PAYIN,
                Currency = w3Ticket.CCY,
                MinTotalOdd = w3Ticket.MIN_TOTAL_ODD,
                MaxTotalOdd = w3Ticket.MAX_TOTAL_ODD,
                TicketStructure = w3Ticket.TICKET_STRUCTURE,
                TicketLines = new List<TicketLine>()
            };
            foreach (var matchType in w3Ticket.MATCH_TYPES)
            {
                State.TicketLine line = new();
                line.MatchId = matchType.MATCH_ID;
                line.MatchCode = matchType.MATCH_CODE;
                line.TicketSystem = matchType.TICKET_SYSTEM;
                line.OddValue = matchType.ODD_VALUE;
                line.OddStatus = matchType.ODD_STATUS;
                line.SpecialValue = matchType.SPECIAL_VALUE;
                line.IsLive = matchType.LIVE == "Y";
                line.TypeCode = matchType.TYPE_CODE;

                ticketNew.TicketLines.Add(line);
            }
            return ticketNew;   
        }

        private Models.TicketView TicketToTicketView(Ticket ticket)
        {
            Models.TicketView ticketView = new()
            {
                Id = ticket.Uuid,
                Code = ticket.Code,
                Type = (int)ticket.Type,
                Source = (int)ticket.Source,
                IsFavourite = ticket.IsFavourite,
                Status = ticket.Status,
                PStatus = ticket.PStatus,
                PayinTime = ticket.PayinTime,
                Payin = ticket.Payin,
                Currency = ticket.Currency,
                MinTotalOdd = ticket.MinTotalOdd,
                MaxTotalOdd = ticket.MaxTotalOdd,
                TicketStructure = ticket.TicketStructure,
                TicketLines = new List<Models.TicketLineView>()
            };

            //if (_importConfig.IbetLocations.Any(x => x ==  ticket.LocationCode))
            //    ticketView.Location = "IBet Default";
            //else
            //{
                var location = _locationService.GetLocation(ticket.LocationCode);
                if (location != null)
                {
                    ticketView.Location = location.Address;
                }
            //}
            
            if (!string.IsNullOrEmpty(ticket.UserUuid))
            {
                var user = _userService.GetUser(ticket.UserUuid);
                if (user != null)
                {
                    ticketView.User = $"{user.FN} {user.LN}";
                    ticketView.UserName = user.UN;
                    ticketView.UserNick = user.NN;
                }
            }
            
            foreach (TicketLine ticketLine in ticket.TicketLines)
            {
                var match = _matchService.GetMatch(ticketLine.MatchId);
                if (match == null)
                    _logger.LogError($"There is no match with ID: {ticketLine.MatchId}");

                var tipType = _tipTypeService.GetTipType(ticketLine.TypeCode);
                
                Models.TicketLineView ticketLineView = new() 
                {
                    MatchCode = ticketLine.MatchCode,
                    IsLive = ticketLine.IsLive,
                    TicketSystem = ticketLine.TicketSystem,
                    OddStatus = ticketLine.OddStatus,
                    OddValue = ticketLine.OddValue,
                    SpecialValue = ticketLine.SpecialValue,
                    BetTypeName = tipType?.Desc,
                    BetTypeShortName = tipType?.Label,
                    Sport = match?.Sport,
                    League = match?.League,
                    Home = match?.Home,
                    Away = match?.Away,
                    Status = match?.Status,
                    Phase = match?.Phase,
                    Round = match?.Round,
                    KickoffTime = match?.KickoffTime,
                    StatusInLive = match?.Live?.Status,
                    PhaseInLive = match?.Live?.Phase,
                };
                ticketView.TicketLines.Add(ticketLineView);
            }
            return ticketView;
        }

        private static string _ticketTemplate = @"{
		""PSTATUS_SORT"": 0,
		""SETTLEMENT_ID"": null,
		""AUTHORIZATION_DATE_TIME"": null,
		""CONFIRM_PRINTED"": null,
		""WIN_TAX_RATE"": 0,
		""APP_NAME"": ""iBet"",
		""HAPPY_HOUR_BONUS"": 0,
		""PAYOUT_CANCEL_TERMINAL"": 11,
		""WHITE_WIN"": null,
		""PIN_CODE"": ""7476"",
		""HAS_BONUS"": ""Y"",
		""PSTATUS"": 0,
		""BONUS_UNLOCK_AMOUNT"": null,
		""AUTHORIZATION_STATUS"": null,
		""CASHOUT_BASE_MARGIN_FRACTION"": 12,
		""CCY_RATE"": 1,
		""FREE_BET"": null,
		""CPAYIN_REASON"": null,
		""ID"": 11606911,
		""MAX_TOTAL_ODD"": 15.33,
		""NUM_OF_MATCHES"": 2,
		""LANGUAGE"": ""sr"",
		""TOTAL_ODD_LIMIT"": 0,
		""PAY_BACK_PARAMS"": ""0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#"",
		""PAY_OUT_DELAY"": 0,
		""PAYOUT_CASHIER"": null,
		""TICKET_TYPE"": ""J"",
		""REFERRAL_GIFT"": null,
		""PAYIN"": 20,
		""AGENT_PERCENT"": null,
		""MTS_LIMIT_ID"": null,
		""PAYOUT_PROMO_CODE"": null,
		""ORDINAL_NUMBER"": 0,
		""OVERRIDDEN_DATE_TIME"": null,
		""MAX_TOTAL_WIN_TAX"": 0,
		""REF_TICKET_UUID"": null,
		""ROUNDING"": 1,
		""BONUS"": null,
		""PAYOUT_LIMIT"": 5000000,
		""CPAYIN_TIME"": null,
		""IS_ADMIN_CPAYIN"": null,
		""WIN_DAY"": null,
		""MIN_TOTAL_BONUS"": 0,
		""GREEN_WIN"": null,
		""LOCATION"": 1,
		""TERMINAL"": 11,
		""FOR_REPLICATION"": null,
		""SCHEDULED_JACKPOT_ID"": null,
		""INSTANCE_CODE"": ""soccercg"",
		""MATCH_TYPES"": [
			{
				""PREPAYMENT_INITIAL_ODD_VALUE"": null,
				""EXPRESSION_VALUES"": null,
				""PREPAYMENT_MARGIN"": null,
				""HAS_BONUS"": ""Y"",
				""LIVE"": ""N"",
				""BET_PICK_GROUP_ID"": 20677,
				""MATCH_CODE"": 58,
				""GRAY_SPECIAL_VALUE"": null,
				""VOID_REASON"": null,
				""BB_EXT_ID"": null,
				""MATCH_BET_ID"": null,
				""TYPE_CODE"": 1,
				""ODD_STATUS"": 0,
				""OVERRIDDEN_ODD_VALUE"": null,
				""WHITE_ODD_VALUE"": null,
				""INITIAL_ODD_VALUE"": null,
				""GRAY_ODD_VALUE"": null,
				""TICKET_ORDER"": 2,
				""ODD_VALUE"": 1.94,
				""CURRENT_VALUES"": null,
				""MATCH_ID"": 475856912,
				""LIVE_TYPE_CODE"": 89080,
				""LEAGUE_CODE"": 175,
				""PREPAYMENT"": null,
				""BET_CODE"": 22579,
				""SPECIAL_VALUE"": null,
				""INITIAL_SPECIAL_VALUE"": null,
				""BET_BUILDER"": null,
				""MATCH_LIVE_BET_ID"": null,
				""TICKET_SYSTEM"": 1,
				""GRAY_DATE_TIME"": null,
				""WHITE_SPECIAL_VALUE"": null,
				""OVERRIDDEN_ODD_STATUS"": null,
				""BB_ODD"": null,
				""BB_EXT_TYPE_ID"": null
			},
			{
				""PREPAYMENT_INITIAL_ODD_VALUE"": null,
				""EXPRESSION_VALUES"": null,
				""PREPAYMENT_MARGIN"": null,
				""HAS_BONUS"": ""Y"",
				""LIVE"": ""Y"",
				""BET_PICK_GROUP_ID"": 20677,
				""MATCH_CODE"": 8188,
				""GRAY_SPECIAL_VALUE"": null,
				""VOID_REASON"": null,
				""BB_EXT_ID"": null,
				""MATCH_BET_ID"": null,
				""TYPE_CODE"": 1,
				""ODD_STATUS"": 0,
				""OVERRIDDEN_ODD_VALUE"": null,
				""WHITE_ODD_VALUE"": null,
				""INITIAL_ODD_VALUE"": null,
				""GRAY_ODD_VALUE"": null,
				""TICKET_ORDER"": 1,
				""ODD_VALUE"": 7.9,
				""CURRENT_VALUES"": ""CMIN=58,CP_1=0,CF=3,CS_2=0,CS_1=0,CP=2,CP_2=0"",
				""MATCH_ID"": 475858258,
				""LIVE_TYPE_CODE"": 87026,
				""LEAGUE_CODE"": 2473,
				""PREPAYMENT"": null,
				""BET_CODE"": 21957,
				""SPECIAL_VALUE"": null,
				""INITIAL_SPECIAL_VALUE"": null,
				""BET_BUILDER"": null,
				""MATCH_LIVE_BET_ID"": 1143980312,
				""TICKET_SYSTEM"": 1,
				""GRAY_DATE_TIME"": null,
				""WHITE_SPECIAL_VALUE"": null,
				""OVERRIDDEN_ODD_STATUS"": null,
				""BB_ODD"": null,
				""BB_EXT_TYPE_ID"": null
			}
		],
		""CTIME"": ""2023-06-16T08:13:17.000+0000"",
		""NUM_OF_UNFINISHED_MATCHES"": 2,
		""PREPAYMENT_UUID"": null,
		""PAYIN_DAY"": ""2023-06-15T22:00:00.000+0000"",
		""PAYOUT_TIME"": null,
		""CARD_TYPE"": 0,
		""PREPAYMENT_BOOKMAKER"": null,
		""HTTP_SESSION_ID"": ""374ce144-7de0-4cb2-bdd5-f215fd5e26bf"",
		""OVERRIDDEN_BOOKMAKER"": null,
		""AGENT"": null,
		""UNLOCKS_FREEBET_BONUS"": null,
		""ACCEPTED_AGENT_PERCENT"": null,
		""REF_USER_UUID"": null,
		""HIT_ODD"": null,
		""TICKET_STRUCTURE"": ""2/2;"",
		""PROMO_PIN"": null,
		""MIN_TOTAL_ODD"": 15.33,
		""INTERNAL_NOTE"": null,
		""PAYOUT_DENIED"": null,
		""NUM_OF_COMBINATIONS"": 1,
		""MAX_TOTAL_BONUS"": 0,
		""AGENT_PROFIT"": null,
		""PREPAYMENT"": null,
		""PAYOUT_AUTHORISED"": ""N"",
		""WIN_EXTRA_TYPE"": null,
		""UNLOCKS_DEPOSIT"": ""Y"",
		""CPAYIN_CASHIER"": null,
		""MD5"": ""3784511a9ceaf767ffe7f6a79630df92"",
		""WHITE_PAYIN"": 20,
		""FOR_PROCESSING"": null,
		""FISCAL_NUMBER"": null,
		""TAX_FREE_WIN_LIMIT"": 100000,
		""CARD_LOCATION"": 0,
		""CONFIRM_PRINTED_TIME"": null,
		""TAX_TYPE"": ""SR"",
		""CONFIRM_PRINTED_USER"": null,
		""VOUCHER_CODES"": null,
		""CANCELLATION_CONFIRMED"": null,
		""RAS_UUID"": null,
		""TICKET_TYPES"": ""0000"",
		""STATUS"": 0,
		""PTIME"": ""2023-06-16T08:13:17.000+0000"",
		""RISK_VALUE"": -17,
		""MIN_TOTAL_WIN"": 307,
		""PAYOUT_PROMO_IMAGE_ID"": null,
		""SUPER7_ODD"": 0,
		""UUID"": ""0fcd6471-3ff4-452a-bd3d-ade40adfb7fd"",
		""EXTRA_BONUS_PERCENT"": 0,
		""AUTHORIZATION_ID"": 0,
		""PAYIN_TIME"": ""2023-06-16T08:13:17.000+0000"",
		""MIN_TOTAL_WIN_TAX"": 0,
		""EXTERNAL_TICKET_UUID"": null,
		""USER_SESSION_ID"": null,
		""JACKPOT_TICKET_TEMPLATE_ID"": null,
		""AUTHORIZATION_BOOKMAKER"": null,
		""CASHOUT_BASE_MARGIN"": 0.072,
		""PROMO_IMAGE_ID"": null,
		""GIFT_UUID"": null,
		""PAYIN_METHOD"": 0,
		""CONTEST_CODE"": null,
		""USER_UUID"": ""ff21d258-bfa1-4084-9284-b9652c149994"",
		""WIN"": null,
		""AGENT_PROFIT_TYPE"": ""N"",
		""POSSIBLE_WIN"": 307,
		""PAYOUT_TO_CARD"": 0,
		""MAX_TOTAL_WIN"": 307,
		""ROUND"": 23021,
		""OPEN_VALUE"": 20,
		""CLIENT_IP_ADDRESS"": ""185.29.101.32"",
		""MASTER_TICKET"": null,
		""AUTO_PAYOUT_LIMIT"": 0,
		""PAYIN_BONUS"": 0,
		""TICKET_SUBTYPE"": ""F"",
		""APP_VERSION"": ""4.54-SNAPSHOT (2.22-SNAPSHOT)"",
		""CBLA_COEF"": 1,
		""PROVISION"": 0,
		""CARD_NUMBER"": 0,
		""WIN_TAX"": null,
		""SERIAL_NUMBER"": ""3979110"",
		""BONUS_VOUCHER_ID"": null,
		""GRAY_WIN"": null,
		""AGENT_PROFIT_PARAMS"": ""0;0;0;0;0;0;0;0;0;0"",
		""TIME_ZONE"": 120,
		""PREPAYMENT_DATE_TIME"": null,
		""PROCESSING_DISABLED"": null,
		""PAYIN_CASHIER"": ""25460555555"",
		""CANCEL_ENABLE"": ""N"",
		""WIN_TIME"": null,
		""CPAYIN_DAY"": null,
		""PAYOUT_DAY"": null,
		""PRINTED"": null,
		""RESOLVED_DAY"": null,
		""PAYOUT"": null,
		""BONUS_PARAMS"": ""35#1#0#2#0#3#0#4#1#5#3#6#5#7#10#8#15#9#20#10#25#11#30#12#35#13#40#14#45#15#50#16#55#17#60#18#70#19#80#20#90#21#100#22#110#23#125#24#140#25#155#26#170#27#185#28#200#29#215#30#230#31#250#32#270#33#290#34#320#35#350#"",
		""BET_BONUS_TO_FREEBET"": null,
		""CODE"": ""1023021-1956630"",
		""PAYOUT_TAX"": null,
		""USER_NOTE"": null,
		""BOOK"": 10,
		""CCY"": ""din"",
		""PROMO_CODE"": null,
		""PAYOUT_CANCEL_LOCATION"": 1,
		""EXTRA_TIME"": 0,
		""ODDS_SUM"": 9.84,
		""PREPAYMENT_ALLOWED"": ""Y"",
		""MTIME"": null
	}";
    }
}
