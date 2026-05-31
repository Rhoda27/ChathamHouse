using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChathamHouse.Models
{
    public class LiveStream
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string StreamTitle { get; set; }

        public string StreamDescription { get; set; }

        public string StreamKey { get; set; } // Unique key for RTMP stream

        public string StreamUrl { get; set; } // Public URL to watch

        public DateTime StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public bool IsLive { get; set; }

        public int ViewerCount { get; set; }

        public int TotalLikes { get; set; }

        public string ThumbnailUrl { get; set; }

        // Navigation properties
        public virtual AppUser User { get; set; }
        public virtual ICollection<StreamComment> Comments { get; set; }
        public virtual ICollection<StreamLike> Likes { get; set; }
        public virtual ICollection<StreamViewer> Viewers { get; set; }
    }

    public class StreamComment
    {
        public int Id { get; set; }
        public int StreamId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual LiveStream Stream { get; set; }
        public virtual AppUser User { get; set; }
    }

    public class StreamLike
    {
        public int Id { get; set; }
        public int StreamId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual LiveStream Stream { get; set; }
        public virtual AppUser User { get; set; }
    }

    public class StreamViewer
    {
        public int Id { get; set; }
        public int StreamId { get; set; }
        public string UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }

        public virtual LiveStream Stream { get; set; }
        public virtual AppUser User { get; set; }
    }
}