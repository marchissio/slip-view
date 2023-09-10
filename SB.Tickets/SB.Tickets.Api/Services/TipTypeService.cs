using SB.Tickets.Api.State;

namespace SB.Tickets.Api.Services
{
    public interface ITipTypeService
    {
        TipType GetTipType(int key);
    }
    public class TipTypeService : ITipTypeService
    {
        private readonly ILogger<TipTypeService> _logger;
        private readonly Dictionary<int, TipType> _cache = new Dictionary<int, TipType>
        {
            { 1, new TipType(1) { Label = "1", Desc = "Domacin pobedjuje", Game = "Konacan ishod", Name = "1" } },
            { 2, new TipType(2) { Label = "X", Desc = "Nereseno", Game = "Konacan ishod", Name = "X" } },
            { 3, new TipType(3) { Label = "2", Desc = "Gost pobedjuje", Game = "Konacan ishod", Name = "2" } },
            //TODO: Dodaj jos igara ako treba
        };

        public TipTypeService(ILogger<TipTypeService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public TipType GetTipType(int key)
        {
            try
            {
                if (_cache.TryGetValue(key, out TipType state))
                {
                    return Clone(state);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetTipType)} key: {key}");
            }
            return null;
        }

        private TipType Clone(TipType state)
        {
            return new TipType(state.Key)
            {
                Label = state.Label,
                Name = state.Name,
                Desc = state.Desc,
                Game = state.Game,
            };
        }
    }
}
