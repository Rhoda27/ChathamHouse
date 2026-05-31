namespace ChathamHouse.Models
{
    public class SavedPost
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Post Post { get; set; }
        public virtual AppUser User { get; set; }
    }
}
