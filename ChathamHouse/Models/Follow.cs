namespace ChathamHouse.Models
{
    public class Follow
    {
        public int Id { get; set; }
        public string FollowerId { get; set; }
        public  AppUser Follower {get; set;}

        public string FollowingId { get; set; }
        public AppUser Following { get; set; }
    }
}
