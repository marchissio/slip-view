
namespace SB.Tickets.Api.Cache.Models
{
    public class League : ICacheItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Country { get; set; }
        public string Sport { get; set; }

        public ICacheItem GetData()
        {
            return new League 
            {
                Id = Id,
                Name = Name,
                ShortName = ShortName,
                Country = Country,
                Sport = Sport
            };
        }
    }
}
