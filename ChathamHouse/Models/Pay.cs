namespace ChathamHouse.Models
{
    public class Pay
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public bool IsSuccessful { get; set; }
        public DateTime PaymentDate { get; set; }
        public int? PostId { get; set; }
        public string TransactionReference { get; set; }

        public virtual AppUser User { get; set; }
        public virtual Post Post { get; set; }
    }
}
