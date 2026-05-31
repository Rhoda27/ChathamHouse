using ChathamHouse.Data;
using ChathamHouse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChathamHouse.Pages.Setting
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IWebHostEnvironment _environment;

        public EditModel(ApplicationDbContext context, SignInManager<AppUser> signInManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _context = context;
            _signInManager = signInManager;
        }

        [BindProperty]
        public Post posts { get; set; }

        [BindProperty]
        public IFormFile? Imagefile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _signInManager.UserManager.GetUserAsync(User);

            if (currentUser == null)
                return NotFound();

            // Load the user's existing post
            posts = await _context.Posts
                .Include(p => p.Profile)
                .FirstOrDefaultAsync(a => a.UserId == currentUser.Id);

            // If user hasn't paid yet, block access
            if (posts == null || string.IsNullOrEmpty(posts.PaymentReference))
                return RedirectToPage("/post/Create");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUser = await _signInManager.UserManager.GetUserAsync(User);

            if (currentUser == null)
                return NotFound();

            // Load the existing post from DB — preserves PaymentReference and IsPaid
            var existingPost = await _context.Posts
                .FirstOrDefaultAsync(a => a.UserId == currentUser.Id);

            if (existingPost == null || string.IsNullOrEmpty(existingPost.PaymentReference))
                return RedirectToPage("/post/Create");

            // Handle image upload if a new image was provided
            if (Imagefile != null && Imagefile.Length > 0)
            {
                // Validate file size (10MB max)
                if (Imagefile.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("Imagefile", "File size must not exceed 10MB");
                    posts = existingPost;
                    return Page();
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var fileExtension = Path.GetExtension(Imagefile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Imagefile", "Only .jpg, .jpeg, .png and .webp images are allowed");
                    posts = existingPost;
                    return Page();
                }

                // Delete old image if it exists and isn't a placeholder
                if (!string.IsNullOrEmpty(existingPost.ImageUrl))
                {
                    var oldImagePath = Path.Combine(
                        _environment.WebRootPath,
                        existingPost.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                // Save new image
                var uploadFolder = Path.Combine(_environment.WebRootPath, "Upload");
                Directory.CreateDirectory(uploadFolder);

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Imagefile.CopyToAsync(stream);
                }

                existingPost.ImageUrl = "/Upload/" + fileName;
            }

            // Update only editable fields — PaymentReference and IsPaid are NEVER touched
            existingPost.Name = posts.Name;
            existingPost.Content = posts.Content;
            existingPost.Location = posts.Location;

            _context.Posts.Update(existingPost);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Post updated successfully!";
            return RedirectToPage("/Land/Index");
        }
    }
}