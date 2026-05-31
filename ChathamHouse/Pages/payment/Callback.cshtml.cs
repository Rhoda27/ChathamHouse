using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChathamHouse.Data;
using ChathamHouse.Models;
using ChathamHouse.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChathamHouse.Pages.Payment
{
    public class CallbackModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaystackService _paystackService;
        private readonly ILogger<CallbackModel> _logger;
        private readonly IWebHostEnvironment _environment;

        // Page properties — set by OnGetAsync, read by the view
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
        public int? PostId { get; set; }

        public CallbackModel(
            ApplicationDbContext context,
            IPaystackService paystackService,
            IWebHostEnvironment environment,
            ILogger<CallbackModel> logger)
        {
            _context = context;
            _paystackService = paystackService;
            _environment = environment;
            _logger = logger;
        }

        public async Task OnGetAsync(string reference)
        {
            if (string.IsNullOrEmpty(reference))
            {
                ErrorMessage = "Invalid payment reference.";
                return;
            }

            _logger.LogInformation("Processing payment callback for reference: {Reference}", reference);

            try
            {
                var verificationResponse = await _paystackService.VerifyPayment(reference);

                if (verificationResponse != null && verificationResponse.Status &&
                    verificationResponse.Data?.Status?.ToLower() == "success")
                {
                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.Reference == reference);

                    if (payment != null && !payment.IsSuccessful)
                    {
                        payment.IsSuccessful = true;
                        payment.TransactionReference = verificationResponse.Data?.Reference ?? reference;
                        payment.PaymentDate = DateTime.UtcNow;

                        if (payment.PostId.HasValue && payment.PostId.Value > 0)
                        {
                            var post = await _context.Posts
                                .FirstOrDefaultAsync(p => p.Id == payment.PostId.Value);

                            if (post != null)
                            {
                                if (!post.IsPaid)
                                {
                                    post.IsPaid = true;
                                    post.PaymentDate = DateTime.UtcNow;
                                    post.PaymentReference = reference;
                                    _logger.LogInformation("Post published. PostId: {PostId}", post.Id);
                                    SuccessMessage = $"Your post '{post.Name}' has been published successfully!";
                                    PostId = post.Id;
                                }
                                else
                                {
                                    SuccessMessage = $"Your post '{post.Name}' was already published.";
                                    PostId = post.Id;
                                }
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                await _context.SaveChangesAsync();
                                _logger.LogWarning("Post not found for ID: {PostId}", payment.PostId);
                                ErrorMessage = "Post not found. Please contact support.";
                            }
                        }
                        else
                        {
                            var post = await _context.Posts
                                .FirstOrDefaultAsync(p => p.PaymentReference == reference);

                            if (post != null)
                            {
                                post.IsPaid = true;
                                post.PaymentDate = DateTime.UtcNow;
                                post.PaymentId = payment.Id;
                                _logger.LogInformation("Post published via reference fallback. PostId: {PostId}", post.Id);
                                SuccessMessage = $"Your post '{post.Name}' has been published successfully!";
                                PostId = post.Id;
                            }
                            else
                            {
                                _logger.LogWarning("No post found for reference: {Reference}", reference);
                                ErrorMessage = "Post not found. Please contact support.";
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                    else if (payment != null && payment.IsSuccessful)
                    {
                        SuccessMessage = "Payment already processed successfully.";
                        PostId = payment.PostId;
                    }
                    else
                    {
                        _logger.LogWarning("Payment record not found for reference: {Reference}", reference);
                        ErrorMessage = "Payment record not found. Please contact support.";
                    }
                }
                else
                {
                    var reason = verificationResponse?.Message ?? "Payment verification failed";
                    _logger.LogWarning("Payment failed. Reference: {Reference}, Reason: {Reason}", reference, reason);
                    ErrorMessage = "Payment was not completed. Please try again or contact support.";

                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.Reference == reference);

                    if (payment != null && !payment.IsSuccessful)
                    {
                        payment.IsSuccessful = false;
                        await _context.SaveChangesAsync();
                        await CleanUpUnpaidPostAsync(reference);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback for reference: {Reference}", reference);
                ErrorMessage = "An error occurred while verifying your payment. Please contact support.";
            }


        }

        private async Task CleanUpUnpaidPostAsync(string reference)
        {
            try
            {
                var post = await _context.Posts
                    .FirstOrDefaultAsync(p => p.PaymentReference == reference && !p.IsPaid);

                if (post == null) return;

                if (!string.IsNullOrEmpty(post.ImageUrl))
                {
                    var webRoot = _environment.WebRootPath
                        ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var fullPath = Path.Combine(webRoot, post.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up unpaid post. Reference: {Reference}", reference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up unpaid post. Reference: {Reference}", reference);
            }
        }
    }
}