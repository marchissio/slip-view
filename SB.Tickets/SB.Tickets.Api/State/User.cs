
namespace SB.Tickets.Api.State
{
    public class User
    {
        public string Uuid { get; }

        /// <summary>
        /// First name
        /// </summary>
        public string FN { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string LN { get; set; }

         /// <summary>
        /// User name
        /// </summary>
        public string UN { get; set; }

         /// <summary>
        /// Nick name
        /// </summary>
        public string NN { get; set; }

        public User(string uuid)
        {
            Uuid = uuid;
        }
    }
}
