using Azure.Identity;
using ChathamHouse.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChathamHouse.Pages.Form
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
       

        public RegisterModel(UserManager<AppUser> userManager)
        {
            
            _userManager = userManager;
        }
        [BindProperty]
        public RegisterViewModel Input { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Force UserName = Email
            var appUser = new AppUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                ClubName = Input.ClubName,
                Location = Input.Location
            };

            var result = await _userManager.CreateAsync(appUser, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return Page();
            }

            await _userManager.AddToRoleAsync(appUser, "User");

            return RedirectToPage("Login");
        }
    }
}
