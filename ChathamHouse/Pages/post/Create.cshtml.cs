using ChathamHouse.Data;
using ChathamHouse.Models;
using ChathamHouse.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChathamHouse.Pages.post
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<AppUser> _userManager;
        private readonly IPaystackService _paystackService;
        private readonly ILogger<CreateModel> _logger;

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

        public CreateModel(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            UserManager<AppUser> userManager,
            IPaystackService paystackService,
            ILogger<CreateModel> logger)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
            _paystackService = paystackService;
            _logger = logger;
        }

        [BindProperty]
        public Post posts { get; set; }

        public string successMessage = "";

        [BindProperty]
        public IFormFile? Imagefile { get; set; }

        [BindProperty]
        public string PaymentReference { get; set; }

        public decimal PaymentAmount { get; set; } = 1000;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrEmpty(PaymentReference))
            {
                return await HandlePaymentCallback();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Form/Login");

            // Validate image exists
            if (Imagefile == null)
            {
                ModelState.AddModelError("Imagefile", "Please select an image");
                return Page();
            }

            // Validate file size
            if (Imagefile.Length > MaxFileSizeBytes)
            {
                ModelState.AddModelError("Imagefile", $"File size must not exceed {MaxFileSizeBytes / (1024 * 1024)}MB");
                return Page();
            }

            // Validate file extension only — no dimension check
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(Imagefile.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("Imagefile", "Only .jpg, .jpeg, .png and .webp images are allowed");
                return Page();
            }

            // Validate post name and content
            if (string.IsNullOrWhiteSpace(posts.Name))
            {
                ModelState.AddModelError("posts.Name", "Name is required");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(posts.Content))
            {
                ModelState.AddModelError("posts.Content", "Content is required");
                return Page();
            }

            try
            {
                string imageUrl = null;
                if (Imagefile != null && Imagefile.Length > 0)
                {
                    var UploadFolder = Path.Combine(_environment.WebRootPath, "Upload");
                    Directory.CreateDirectory(UploadFolder);

                    var FileName = $"{Guid.NewGuid()}_{Path.GetFileName(Imagefile.FileName)}";
                    var FilePath = Path.Combine(UploadFolder, FileName);

                    using (var Stream = new FileStream(FilePath, FileMode.Create))
                    {
                        await Imagefile.CopyToAsync(Stream);
                    }
                    imageUrl = "/Upload/" + FileName;
                }

                var reference = $"POST_{DateTime.Now.Ticks}_{user.Id}_{Guid.NewGuid().ToString().Substring(0, 8)}";

                var paymentRequest = new PaymentRequest
                {
                    Email = user.Email,
                    Amount = PaymentAmount,
                    Reference = reference,
                    UserId = user.Id,
                    PostTitle = posts.Name
                };

                _logger.LogInformation("Initializing payment for user {UserId}, post title: {Title}", user.Id, posts.Name);

                var paymentResponse = await _paystackService.InitializePayment(paymentRequest);

                if (paymentResponse != null && paymentResponse.Status)
                {
                    var newPost = new Post
                    {
                        UserId = user.Id,
                        Name = posts.Name,
                        Content = posts.Content,
                        Location = posts.Location,
                        ImageUrl = imageUrl,
                        CreatedAt = DateTime.UtcNow,
                        IsPaid = false,
                        PaymentReference = reference,
                        LikeCount = 0,
                        CommentCount = 0
                    };

                    await _context.Posts.AddAsync(newPost);
                    await _context.SaveChangesAsync();

                    var payment = new Pay
                    {
                        UserId = user.Id,
                        Reference = reference,
                        Amount = PaymentAmount,
                        IsSuccessful = false,
                        PaymentDate = DateTime.UtcNow,
                        PostId = newPost.Id,
                        TransactionReference = reference
                    };

                    await _context.Payments.AddAsync(payment);
                    await _context.SaveChangesAsync();

                    newPost.PaymentId = payment.Id;
                    await _context.SaveChangesAsync();

                    TempData["PendingPostId"] = newPost.Id;
                    TempData["PaymentReference"] = reference;

                    _logger.LogInformation("Redirecting to Paystack payment page. Reference: {Reference}", reference);

                    return Redirect(paymentResponse.Data.AuthorizationUrl);
                }
                else
                {
                    var errorMessage = paymentResponse?.Message ?? "Unable to initialize payment";
                    ModelState.AddModelError("", $"Payment initialization failed: {errorMessage}");
                    _logger.LogWarning("Payment initialization failed: {Message}", errorMessage);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post with payment for user {UserId}", user.Id);
                ModelState.AddModelError("", $"Error: {ex.Message} | Inner: {ex.InnerException?.Message} | Inner2: {ex.InnerException?.InnerException?.Message}");
                return Page();
            }
        }

        private async Task<IActionResult> HandlePaymentCallback()
        {
            try
            {
                _logger.LogInformation("Processing payment callback for reference: {Reference}", PaymentReference);

                var verificationResponse = await _paystackService.VerifyPayment(PaymentReference);

                if (verificationResponse != null && verificationResponse.Status &&
                    verificationResponse.Data?.Status?.ToLower() == "success")
                {
                    var postId = TempData["PendingPostId"] as int?;
                    Post existingPost = null;

                    if (postId.HasValue)
                    {
                        existingPost = await _context.Posts.FindAsync(postId.Value);
                    }

                    if (existingPost == null)
                    {
                        existingPost = await _context.Posts
                            .FirstOrDefaultAsync(p => p.PaymentReference == PaymentReference);
                    }

                    if (existingPost != null)
                    {
                        existingPost.IsPaid = true;
                        existingPost.PaymentDate = DateTime.UtcNow;
                        existingPost.PaymentReference = PaymentReference;
                        await _context.SaveChangesAsync();
                    }

                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.Reference == PaymentReference);

                    if (payment != null)
                    {
                        payment.IsSuccessful = true;
                        payment.TransactionReference = verificationResponse.Data?.Reference ?? PaymentReference;
                        payment.PaymentDate = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }

                    successMessage = "Payment successful! Your post has been published.";
                    _logger.LogInformation("Payment successful for reference: {Reference}", PaymentReference);

                    TempData.Remove("PendingPostId");
                    TempData.Remove("PaymentReference");

                    return RedirectToPage("/Land/Index");
                }
                else
                {
                    var errorMsg = verificationResponse?.Message ?? "Payment verification failed";
                    TempData["Error"] = $"Payment failed: {errorMsg}";
                    _logger.LogWarning("Payment verification failed for reference: {Reference}", PaymentReference);

                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.Reference == PaymentReference);

                    if (payment != null)
                    {
                        payment.IsSuccessful = false;
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToPage("/Land/Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment callback for reference: {Reference}", PaymentReference);
                TempData["Error"] = "An error occurred while processing your payment. Please contact support.";
                return RedirectToPage("/Land/Index");
            }
        }

        public void OnGet()
        {
        }
    }
}