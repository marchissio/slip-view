
namespace SB.Tickets.Api.State
{
    public class TipType
    {
        public int Key { get; }
        public string Label { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Game { get; set; }

        public TipType(int key)
        {
            Key = key;
        }
    }
}
