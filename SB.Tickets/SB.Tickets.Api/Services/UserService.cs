using SB.Tickets.Api.State;

namespace SB.Tickets.Api.Services
{
    public interface IUserService
    {
        User GetUser(string uuid);
    }
    
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly Dictionary<string, User> _userCache = new Dictionary<string, User> 
        {
            { "ff21d258-bfa1-4084-9284-b9652c149994", new User("ff21d258-bfa1-4084-9284-b9652c149994") { FN = "Marko", LN = "Milosevic", NN = "Mare", UN = "m.milosevic" } }
            //TODO: Dodaj jos korisnika ako treba
        };

        public UserService(ILogger<UserService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public User GetUser(string uuid)
        {
            try
            {
                if (_userCache.TryGetValue(uuid, out User user))
                {
                    return CloneUser(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(GetUser));
            }
            return null;
        }

        private User CloneUser(User user)
        {
            return new User(user.Uuid)
            { 
                FN = user.FN,
                LN = user.LN,
                UN = user.UN,
                NN = user.NN,
            };
        }
    }
}
