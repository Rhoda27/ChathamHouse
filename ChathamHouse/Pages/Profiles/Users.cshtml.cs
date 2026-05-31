using ChathamHouse.Data;
using ChathamHouse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChathamHouse.Pages.Profiles
{
    [Authorize]
    public class UsersModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UsersModel(UserManager<AppUser> userManager, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public AppUser ProfileUser { get; set; }
        [BindProperty]
        public Profile Profile { get; set; }
        public List<Post> posts { get; set; }
        public List<PostViewModel> Posts { get; set; }
        public int PostsCount { get; set; }
        public int followerCount { get; set; }
        public int folowingCount { get; set; }
        public bool IsFollowing { get; set; }

        public string CurrentUserId { get; set; }


        [BindProperty]
        public IFormFile ImageFile { get; set; }

        // --- Load profile ---
        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                id = _userManager.GetUserId(User);

            var currentUserId = _userManager.GetUserId(User);

            var userPosts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.SavedByUsers)
                .Where(p => p.UserId == id && p.IsPaid)
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .ToListAsync();

            Posts = userPosts.Select(p => new PostViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                Content = p.Content,
                Location = p.Location,
                CreatedAt = p.CreatedAt,
                LikeCount = p.Likes?.Count ?? 0,
                CommentCount = p.Comments?.Count ?? 0,
                IsLikedByUser = currentUserId != null && p.Likes != null && p.Likes.Any(l => l.UserId == currentUserId),
                IsSavedByUser = currentUserId != null && p.SavedByUsers != null && p.SavedByUsers.Any(s => s.UserId == currentUserId),
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

            ProfileUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (ProfileUser == null)
                return RedirectToPage("/Land/Index");

            posts = userPosts.ToList();
            PostsCount = posts.Count;

            followerCount = await _context.Follow.CountAsync(f => f.FollowingId == id);
            folowingCount = await _context.Follow.CountAsync(f => f.FollowerId == id);
            IsFollowing = await _context.Follow.AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);

            return Page();
        }

        // --- Follow/Unfollow (page form post - keeps working for profile page button) ---
        public async Task<IActionResult> OnPostFollowAsync(string userId)
        {
            var currentId = _userManager.GetUserId(User);
            if (currentId == userId)
                return RedirectToPage(new { id = userId });

            var existing = await _context.Follow
                .FirstOrDefaultAsync(f => f.FollowerId == currentId && f.FollowingId == userId);

            if (existing == null)
                _context.Follow.Add(new Follow { FollowerId = currentId, FollowingId = userId });
            else
                _context.Follow.Remove(existing);

            await _context.SaveChangesAsync();
            return RedirectToPage(new { id = userId });
        }

        // --- Follow/Unfollow API (called by fetch() from Index/Land pages) ---
        public async Task<IActionResult> OnPostFollowApiAsync([FromBody] FollowRequest request)
        {
            var currentId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentId))
                return new JsonResult(new { success = false, message = "Not logged in" })
                { StatusCode = 401 };

            if (currentId == request.UserId)
                return new JsonResult(new { success = false, message = "Cannot follow yourself" });

            var existing = await _context.Follow
                .FirstOrDefaultAsync(f => f.FollowerId == currentId && f.FollowingId == request.UserId);

            bool isNowFollowing;

            if (existing == null)
            {
                _context.Follow.Add(new Follow { FollowerId = currentId, FollowingId = request.UserId });
                isNowFollowing = true;
            }
            else
            {
                _context.Follow.Remove(existing);
                isNowFollowing = false;
            }

            await _context.SaveChangesAsync();

            // Return updated follower count
            var followerCount = await _context.Follow.CountAsync(f => f.FollowingId == request.UserId);

            return new JsonResult(new
            {
                success = true,
                following = isNowFollowing,
                followerCount = followerCount
            });
        }

        // --- Upload profile image ---
        public async Task<IActionResult> OnPostUploadImageAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (ImageFile != null && user != null)
            {
                try
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Invalid file type. Please upload JPG, PNG, GIF, or WEBP.";
                        return RedirectToPage(new { id = user.Id });
                    }

                    var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                    if (!allowedContentTypes.Contains(ImageFile.ContentType.ToLower()))
                    {
                        TempData["Error"] = "Invalid file content type.";
                        return RedirectToPage(new { id = user.Id });
                    }

                    if (ImageFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "File size must be less than 5MB.";
                        return RedirectToPage(new { id = user.Id });
                    }

                    if (!string.IsNullOrEmpty(user.Image) &&
                        user.Image != "/images/default.png" &&
                        !user.Image.Contains("default.png"))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath,
                            user.Image.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images/profiles");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    user.Image = "/images/profiles/" + fileName;
                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                        TempData["Success"] = "Profile image updated successfully!";
                    else
                    {
                        TempData["Error"] = "Failed to update user profile.";
                        if (System.IO.File.Exists(filePath))
                            System.IO.File.Delete(filePath);
                    }
                }
                catch (Exception)
                {
                    TempData["Error"] = "An error occurred while uploading the image.";
                    return RedirectToPage(new { id = user.Id });
                }

                return RedirectToPage(new { id = user.Id });
            }

            TempData["Error"] = "No file selected or user not found.";
            return RedirectToPage("/Index");
        }
    }

    // --- Request model for follow API ---
    public class FollowRequest
    {
        public string UserId { get; set; }
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