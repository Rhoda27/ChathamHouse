using ChathamHouse.Data;
using ChathamHouse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChathamHouse.Pages.Land
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<AppUser> _signInManager;

        public IndexModel(ApplicationDbContext context, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _signInManager = signInManager;
        }

        public List<LandPostViewModel> Posts { get; set; }
        public AppUser ProfileUser { get; set; }
        public string CurrentUserId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ProfileUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == CurrentUserId);

            // Get IDs of people the current user follows
            var followingIds = await _context.Follow
                .Where(f => f.FollowerId == CurrentUserId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            // Show posts ONLY from people the user follows
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.SavedByUsers)
                .Where(p => p.IsPaid && (followingIds.Contains(p.UserId) || p.UserId == CurrentUserId))
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .ToListAsync();

            Posts = posts.Select(p => new LandPostViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                Content = p.Content,
                Location = p.Location,
                CreatedAt = p.CreatedAt,
                LikeCount = p.Likes?.Count ?? 0,
                CommentCount = p.Comments?.Count ?? 0,
                IsLikedByUser = p.Likes?.Any(l => l.UserId == CurrentUserId) ?? false,
                IsSavedByUser = p.SavedByUsers?.Any(s => s.UserId == CurrentUserId) ?? false,
                UserImage = p.User?.Image,
                UserName = p.User?.UserName,
                PostUserId = p.UserId,
                RecentComments = p.Comments?
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(3)
                    .Select(c => new LandCommentViewModel
                    {
                        Id = c.Id,
                        Content = c.Content,
                        UserName = c.User?.UserName ?? "Anonymous",
                        CreatedAt = c.CreatedAt
                    })
                    .ToList() ?? new List<LandCommentViewModel>()
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostSignOutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Form/Login");
        }
    }

    public class LandPostViewModel
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
        public string UserImage { get; set; }
        public string UserName { get; set; }
        public string PostUserId { get; set; }
        public List<LandCommentViewModel> RecentComments { get; set; }
    }

    public class LandCommentViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}