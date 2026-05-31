public class RatingUserViewModel
{
    public int RatingId { get; set; }
    public string UserId { get; set; }
    public string ClubName { get; set; }
    public string UserEmail { get; set; }
    public string? ProfileImage { get; set; } // Changed from UserAvatarUrl
    public int Stars { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeAgo => GetTimeAgo(CreatedAt);

    private string GetTimeAgo(DateTime date)
    {
        var timeSpan = DateTime.UtcNow - date;
        if (timeSpan.Days > 7) return date.ToString("MMM dd, yyyy");
        if (timeSpan.Days > 1) return $"{timeSpan.Days} days ago";
        if (timeSpan.Days == 1) return "yesterday";
        if (timeSpan.Hours > 1) return $"{timeSpan.Hours} hours ago";
        if (timeSpan.Hours == 1) return "1 hour ago";
        if (timeSpan.Minutes > 1) return $"{timeSpan.Minutes} minutes ago";
        return "just now";
    }
}