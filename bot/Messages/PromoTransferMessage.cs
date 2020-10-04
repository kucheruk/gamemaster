namespace gamemaster.Messages
{
    public class PromoTransferMessage
    {
        public string FromUser { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string ResponseUrl { get; set; }
        public string ToUser { get; set; }
    }
}