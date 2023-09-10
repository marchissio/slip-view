
namespace SB.Tickets.Import.Models
{
    public interface ITable
    {
        /// <summary>
        /// Unique table id if it exists. Otherwise, null.
        /// </summary>
        public string UniqueId { get; }
    }
}
