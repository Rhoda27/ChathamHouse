using Microsoft.AspNetCore.Identity;

namespace ChathamHouse.Models
{
    public class AppUser:IdentityUser
    {
        public string ClubName { get; set; }
        public string Location { get; set; }

        public string? Image { get; set; }

        public ICollection<Post> Posts { get; set; }   // Posts the user created
        public ICollection<Follow> Followers { get; set; }  // People who follow this user
        public ICollection<Follow> Following { get; set; }

        public virtual ICollection<Pay> payment { get; set; }
        public virtual ICollection<LiveStream> LiveStreams { get; set; } = new List<LiveStream>();
        public virtual ICollection<StreamComment> StreamComments { get; set; } = new List<StreamComment>();
        public virtual ICollection<StreamLike> StreamLikes { get; set; } = new List<StreamLike>();
        public virtual ICollection<StreamViewer> StreamViewers { get; set; } = new List<StreamViewer>();
    }
}
