using System.ComponentModel.DataAnnotations;

namespace ChathamHouse.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ImageUrl { get; set; }
        public string Content { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PaymentReference { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsLikedByUser { get; set; }
        public bool IsSavedByUser { get; set; }
        public int? ProfileId { get; set; }


        public virtual Profile Profile { get; set; }

        public virtual AppUser User { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<SavedPost> SavedByUsers { get; set; }
        public int? PaymentId { get; set; }
        public Pay payment { get; set; }


        public double AverageStars { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
        public int OneStar { get; set; } = 0;
        public int TwoStar { get; set; } = 0;
        public int ThreeStar { get; set; } = 0;
        public int FourStar { get; set; } = 0;
        public int FiveStar { get; set; } = 0;

        // Navigation property
        public virtual ICollection<Rating> Ratings { get; set; }
    }
}
