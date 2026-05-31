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

namespace ChathamHouse.Pages.Profiles
{
    [Authorize]
    public class EditProfileModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EditProfileModel(UserManager<AppUser> userManager, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public AppUser ProfileUser { get; set; }
        public Profile UserProfile { get; set; }

        [BindProperty]
        public ProfileEditInput Input { get; set; }

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        // --- Load current profile data ---
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Form/Login");

            ProfileUser = user;

            // Load existing profile or create empty one
            UserProfile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            // Pre-fill form from Profile table
            Input = new ProfileEditInput
            {
                UserName = user.UserName,
                Email = user.Email,
                ClubName = user.ClubName,
                Bio = UserProfile?.Bio,
                ContactAddress = UserProfile?.ContactAddress,
                PhoneNumber = UserProfile?.PhoneNumber,
            };

            return Page();
        }

        // --- Save profile changes ---
        public async Task<IActionResult> OnPostEditProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Form/Login");

            ProfileUser = user;

            if (!ModelState.IsValid)
                return Page();

            // Handle profile picture upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageFile", "Invalid file type. Please upload JPG, PNG, GIF, or WEBP.");
                    return Page();
                }

                var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedContentTypes.Contains(ImageFile.ContentType.ToLower()))
                {
                    ModelState.AddModelError("ImageFile", "Invalid file content type.");
                    return Page();
                }

                if (ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "File size must be less than 5MB.");
                    return Page();
                }

                // Delete old profile image if it exists
                if (!string.IsNullOrEmpty(user.Image) && !user.Image.Contains("default.png"))
                {
                    var oldPath = Path.Combine(
                        _environment.WebRootPath,
                        user.Image.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // Save new image
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images/profiles");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                // Save image path to AppUser
                user.Image = "/images/profiles/" + fileName;
            }

            // Update username if changed
            if (user.UserName != Input.UserName)
            {
                var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.UserName);
                if (!setUserNameResult.Succeeded)
                {
                    foreach (var error in setUserNameResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return Page();
                }
            }

            // Update email if changed
            if (user.Email != Input.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return Page();
                }
            }

            // Update ClubName on AppUser
            user.ClubName = Input.ClubName;
            await _userManager.UpdateAsync(user);

            // Update or create Profile record
            var profile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                // Create new profile if one doesn't exist
                profile = new Profile
                {
                    UserId = user.Id,
                    UserName = Input.UserName,
                    Email = Input.Email,
                    Bio = Input.Bio,
                    ContactAddress = Input.ContactAddress,
                    PhoneNumber = Input.PhoneNumber,
                };
                _context.Profiles.Add(profile);
            }
            else
            {
                // Update existing profile
                profile.UserName = Input.UserName;
                profile.Email = Input.Email;
                profile.Bio = Input.Bio;
                profile.ContactAddress = Input.ContactAddress;
                profile.PhoneNumber = Input.PhoneNumber;
                _context.Profiles.Update(profile);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToPage("/Profiles/Users");
        }
    }

    // --- Input model ---
    public class ProfileEditInput
    {
        public string? ClubName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public string? ContactAddress { get; set; }
        public string? PhoneNumber { get; set; }
    }
}