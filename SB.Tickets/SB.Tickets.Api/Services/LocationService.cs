using SB.Tickets.Api.State;

namespace SB.Tickets.Api.Services
{
    public interface ILocationService
    {
        Location GetLocation(int code);
    }
    
    public class LocationService : ILocationService
    {
        private readonly ILogger<LocationService> _logger;
        private readonly Dictionary<int, Location> _locationCache = new Dictionary<int, Location>
        {
            { 1, new Location(1) { Closed = false, Address = "Petlovo brdo", CountryCode = "RS", GroupName = "Beograd" } }
            //TODO: Dodaj jos lokacija ako treba
        };

        public LocationService(ILogger<LocationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Location GetLocation(int code)
        {
            try
            {
                if (_locationCache.TryGetValue(code, out Location location))
                {
                    return CloneLocation(location);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetLocation)} code: {code}");
            }
            return null;
        }

        private Location CloneLocation(Location location)
        {
            return new Location(location.Code)
            {
                Address = location.Address,
                CountryCode = location.CountryCode,
                GroupName = location.GroupName,
                Closed = location.Closed,
            };
        }
    }
}
