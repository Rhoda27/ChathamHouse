using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ChathamHouse.Data;
using ChathamHouse.Models;

namespace ChathamHouse.LikeController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("{postId}/like")]
        public async Task<IActionResult> LikePost(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return NotFound();

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }
            else
            {
                _context.Likes.Add(new Like
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.Likes.CountAsync(l => l.PostId == postId);

            return Ok(new
            {
                success = true,
                liked = existingLike == null,
                likeCount = likeCount
            });
        }

        [HttpPost("{postId}/comment")]
        public async Task<IActionResult> AddComment(int postId, [FromBody] CommentRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return NotFound();

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var commentCount = await _context.Comments.CountAsync(c => c.PostId == postId);

            return Ok(new
            {
                success = true,
                commentId = comment.Id,
                commentCount = commentCount,
                comment = new
                {
                    id = comment.Id,
                    content = comment.Content,
                    userName = User.Identity.Name,
                    createdAt = comment.CreatedAt
                }
            });
        }

        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    id = c.Id,
                    content = c.Content,
                    userName = c.User.UserName,
                    createdAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPost("{postId}/save")]
        public async Task<IActionResult> SavePost(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return NotFound();

            var existingSave = await _context.SavedPosts
                .FirstOrDefaultAsync(s => s.PostId == postId && s.UserId == userId);

            if (existingSave != null)
            {
                _context.SavedPosts.Remove(existingSave);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, saved = false });
            }
            else
            {
                _context.SavedPosts.Add(new SavedPost
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                return Ok(new { success = true, saved = true });
            }
        }
    }

    public class CommentRequest
    {
        public string Content { get; set; }
    }
}