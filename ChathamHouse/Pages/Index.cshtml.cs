using ChathamHouse.Data;
using ChathamHouse.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChathamHouse.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PostViewModel> Posts { get; set; }
        public AppUser ProfileUser { get; set; }
        public string CurrentUserId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                ProfileUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == CurrentUserId);
            }

            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.SavedByUsers)
                .Where(p => p.IsPaid)
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .ToListAsync();

            Posts = posts.Select(p => new PostViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                Content = p.Content,
                Location = p.Location,
                CreatedAt = p.CreatedAt,
                LikeCount = p.Likes?.Count ?? 0,
                CommentCount = p.Comments?.Count ?? 0,
                IsLikedByUser = CurrentUserId != null && (p.Likes?.Any(l => l.UserId == CurrentUserId) ?? false),
                IsSavedByUser = CurrentUserId != null && (p.SavedByUsers?.Any(s => s.UserId == CurrentUserId) ?? false),
                IsFollowedByUser = CurrentUserId != null && _context.Follow.Any(f => f.FollowerId == CurrentUserId && f.FollowingId == p.UserId),
                UserImage = p.User?.Image,
                UserName = p.User?.UserName,
                PostUserId = p.UserId,
                RecentComments = p.Comments?
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(3)
                    .Select(c => new CommentViewModel
                    {
                        Id = c.Id,
                        Content = c.Content,
                        UserName = c.User?.UserName ?? "Anonymous",
                        CreatedAt = c.CreatedAt
                    })
                    .ToList() ?? new List<CommentViewModel>()
            }).ToList();

            return Page();
        }
    }

    public class PostViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Content { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsLikedByUser { get; set; }
        public bool IsSavedByUser { get; set; }
        public bool IsFollowedByUser { get; set; }
        public string UserImage { get; set; }
        public string UserName { get; set; }
        public string PostUserId { get; set; }
        public List<CommentViewModel> RecentComments { get; set; }
    }

    public class CommentViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}