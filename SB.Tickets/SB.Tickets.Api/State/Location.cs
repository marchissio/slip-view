
namespace SB.Tickets.Api.State
{
    public class Location
    {
        public int Code { get; }
        public string Address { get; set; }
        public string GroupName { get; set; }
        public string CountryCode { get; set; }
        public bool Closed { get; set; }

        public Location(int code)
        {
            Code = code;
        }
    }
}
