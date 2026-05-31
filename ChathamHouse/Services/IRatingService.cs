using System.Threading.Tasks;
using ChathamHouse.Models;

namespace ChathamHouse.Services
{
    public interface IRatingService
    {
        Task<(bool Success, string Message)> AddOrUpdateRating(int postId, string userId, int stars, string comment = null);  // ADD comment parameter
        Task<double> GetAverageStars(int postId);
        Task<int> GetUserStars(int postId, string userId);
        Task<bool> HasUserRated(int postId, string userId);
        Task<PostRatingDto> GetPostRatingDetails(int postId);
    }

    public class PostRatingDto
    {
        public double AverageStars { get; set; }
        public int TotalRatings { get; set; }
        public int OneStar { get; set; }
        public int TwoStar { get; set; }
        public int ThreeStar { get; set; }
        public int FourStar { get; set; }
        public int FiveStar { get; set; }
    }
}