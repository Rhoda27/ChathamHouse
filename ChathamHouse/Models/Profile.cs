namespace ChathamHouse.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string UserId { get; set; }
         public string UserName { get; set; }
        public string Bio { get; set; }
        public string ContactAddress { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? ProfileImage { get; set; }

        public ICollection<Post> Posts { get; set; }
    }
}
