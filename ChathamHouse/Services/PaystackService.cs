using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChathamHouse.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChathamHouse.Services
{
    public class PaystackService : IPaystackService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaystackService> _logger;

        public PaystackService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PaystackService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PaystackResponse> InitializePayment(PaymentRequest request)
        {
            try
            {
                var payload = new
                {
                    email = request.Email,
                    amount = (int)(request.Amount * 100), // Convert to kobo
                    reference = request.Reference,
                    currency = "NGN",
                    callback_url = $"{_configuration["AppBaseUrl"]}/Payment/Callback",
                    metadata = new
                    {
                        user_id = request.UserId,
                        post_title = request.PostTitle,
                        custom_fields = new[]
                        {
                            new
                            {
                                display_name = "User ID",
                                variable_name = "user_id",
                                value = request.UserId
                            },
                            new
                            {
                                display_name = "Post Title",
                                variable_name = "post_title",
                                value = request.PostTitle
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));

                var response = await _httpClient.PostAsync("/transaction/initialize", content);
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Paystack initialization response: {Response}", responseString);

                return JsonSerializer.Deserialize<PaystackResponse>(responseString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Paystack payment");
                return new PaystackResponse { Status = false, Message = ex.Message };
            }
        }

        public async Task<PaystackVerifyResponse> VerifyPayment(string reference)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/transaction/verify/{reference}");
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Paystack verification response: {Response}", responseString);

                return JsonSerializer.Deserialize<PaystackVerifyResponse>(responseString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Paystack payment for reference: {Reference}", reference);
                return new PaystackVerifyResponse { Status = false, Message = ex.Message };
            }
        }
    }
}