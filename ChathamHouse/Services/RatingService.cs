using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChathamHouse.Data;
using ChathamHouse.Models;

namespace ChathamHouse.Services
{
    public class RatingService : IRatingService
    {
        private readonly ApplicationDbContext _context;

        public RatingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> AddOrUpdateRating(
            int postId, string userId, int stars, string? comment = null)
        {
            try
            {
                if (stars < 1 || stars > 5)
                    return (false, "Stars must be between 1 and 5");

                var existingRating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);

                if (existingRating != null)
                {
                    existingRating.Stars = stars;
                    existingRating.UpdatedAt = DateTime.UtcNow;

                    // Only overwrite comment if a new one was provided
                    if (!string.IsNullOrWhiteSpace(comment))
                        existingRating.Comment = comment;
                }
                else
                {
                    var rating = new Rating
                    {
                        PostId = postId,
                        UserId = userId,
                        Stars = stars,
                        Comment = comment,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Ratings.AddAsync(rating);
                }

                await _context.SaveChangesAsync();

                // Only update post statistics for real posts, not website ratings (postId == 0)
                if (postId > 0)
                    await UpdatePostStarStatistics(postId);

                return (true, "Rating submitted successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        private async Task UpdatePostStarStatistics(int postId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.PostId == postId)
                .ToListAsync();

            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.TotalRatings = ratings.Count;
                post.AverageStars = ratings.Any() ? ratings.Average(r => r.Stars) : 0;
                post.OneStar = ratings.Count(r => r.Stars == 1);
                post.TwoStar = ratings.Count(r => r.Stars == 2);
                post.ThreeStar = ratings.Count(r => r.Stars == 3);
                post.FourStar = ratings.Count(r => r.Stars == 4);
                post.FiveStar = ratings.Count(r => r.Stars == 5);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<double> GetAverageStars(int postId)
        {
            if (postId == 0)
            {
                var ratings = await _context.Ratings
                    .Where(r => r.PostId == 0)
                    .ToListAsync();
                return ratings.Any() ? ratings.Average(r => r.Stars) : 0;
            }

            var post = await _context.Posts.FindAsync(postId);
            return post?.AverageStars ?? 0;
        }

        public async Task<int> GetUserStars(int postId, string userId)
        {
            var rating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);
            return rating?.Stars ?? 0;
        }

        public async Task<bool> HasUserRated(int postId, string userId)
        {
            return await _context.Ratings
                .AnyAsync(r => r.PostId == postId && r.UserId == userId);
        }

        public async Task<PostRatingDto> GetPostRatingDetails(int postId)
        {
            if (postId == 0)
            {
                var ratings = await _context.Ratings
                    .Where(r => r.PostId == 0)
                    .ToListAsync();

                return new PostRatingDto
                {
                    AverageStars = ratings.Any() ? ratings.Average(r => r.Stars) : 0,
                    TotalRatings = ratings.Count,
                    OneStar = ratings.Count(r => r.Stars == 1),
                    TwoStar = ratings.Count(r => r.Stars == 2),
                    ThreeStar = ratings.Count(r => r.Stars == 3),
                    FourStar = ratings.Count(r => r.Stars == 4),
                    FiveStar = ratings.Count(r => r.Stars == 5)
                };
            }

            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return new PostRatingDto();

            return new PostRatingDto
            {
                AverageStars = post.AverageStars,
                TotalRatings = post.TotalRatings,
                OneStar = post.OneStar,
                TwoStar = post.TwoStar,
                ThreeStar = post.ThreeStar,
                FourStar = post.FourStar,
                FiveStar = post.FiveStar
            };
        }
    }
}