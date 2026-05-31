using ChathamHouse.Data;
using ChathamHouse.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChathamHouse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public RatingController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("ratings")]
        public async Task<IActionResult> GetRatings()
        {
            try
            {
                var ratings = await _context.Ratings
                    .Where(r => r.PostId == null)  // ← not == 0
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        Id = r.Id,
                        Stars = r.Stars,
                        Comment = r.Comment ?? string.Empty,
                        CreatedAt = r.CreatedAt,
                        // Try to get user info if the UserId matches an existing user
                        UserName = _context.Users.Where(u => u.Id == r.UserId).Select(u => u.ClubName ?? u.UserName).FirstOrDefault() ?? "Anonymous",
                        UserImage = _context.Users.Where(u => u.Id == r.UserId).Select(u => u.Image).FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(ratings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var ratings = await _context.Ratings
    .Where(r => r.PostId == null)  // ← not == 0
    .ToListAsync();

                var stats = new
                {
                    AverageStars = ratings.Any() ? Math.Round(ratings.Average(r => r.Stars), 1) : 0,
                    TotalRatings = ratings.Count
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("save")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveRating([FromForm] int stars, [FromForm] string comment)
        {
            try
            {
                if (stars < 1 || stars > 5)
                {
                    return BadRequest(new { success = false, message = "Please select 1-5 stars" });
                }

                if (string.IsNullOrWhiteSpace(comment))
                {
                    return BadRequest(new { success = false, message = "Please enter a comment" });
                }

                // Try to get the logged-in user
                string userId;
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    userId = user?.Id ?? $"anon_{Guid.NewGuid().ToString().Substring(0, 8)}";
                }
                else
                {
                    userId = $"anon_{Guid.NewGuid().ToString().Substring(0, 8)}";
                }

                var rating = new Rating
                {
                    PostId = null,  // ← not 0
                    UserId = userId,
                    Stars = stars,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Ratings.AddAsync(rating);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Rating posted successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}