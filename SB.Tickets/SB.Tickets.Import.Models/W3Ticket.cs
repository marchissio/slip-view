namespace SB.Tickets.Import.Models
{
    public class W3Ticket : ITable
    {
        public string UniqueId { get { return UUID; } }
        public string UUID { get; set; } //Primary key - 36 characters
        public long ID { get; set; } //W3_TICKET_SEQ.NEXTVAL - internal ticket ID on the server
        public DateTime CTIME { get; set; } //The time the ticket arrived on the server
        public string CODE { get; set; } //Ticket code from the paid in place
        public string INSTANCE_CODE { get; set; } //(od_instance.code)
        public string USER_UUID { get; set; } //Players user uuid(ic_user.uuid)
        public int LOCATION { get; set; } //Ticket location code - Reference to CoLocation.CODE
        public int TERMINAL { get; set; } //(C_TERMINAL).ID terminal id
        public int STATUS { get; set; } //0 paid in, 1 paid out, 2 canceled, 0 uplacen, 1 isplacen, 2 storniran, 3 odbijen (na autorizaciji)
        public int PSTATUS {get;set;} // PROCESSING STATUS: -1 dropped 0 live 1 won,  -1 gubitan, 0, aktivan, 1 dobitan
        public decimal MIN_TOTAL_ODD { get; set; } //Minimum total odds on pay in
        public decimal MAX_TOTAL_ODD { get; set; } //Maximum total odds on pay in
        public decimal PAYIN { get; set; } //Payment amount
        public string CCY { get; set; } //The currency in which the ticket was paid in
        public DateTime? PAYIN_TIME { get; set; } //The date and time when the ticket was paid in
        public string TICKET_TYPE { get; set; } //X x-bet, B - vegas bet, L - vegas live bet
        public string TICKET_STRUCTURE {get; set; } //Ticket structure: 3/3; 3/2/1; 4/3/1; [<number of matches> [/<number of hits>];]
        public List<MatchType> MATCH_TYPES { get; set; }
    }
}
