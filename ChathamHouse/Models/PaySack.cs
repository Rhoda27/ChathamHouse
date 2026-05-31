using System.Text.Json.Serialization;

namespace ChathamHouse.Models
{
    public class PaystackResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public PaystackData Data { get; set; }
    }

    public class PaystackData
    {
        [JsonPropertyName("authorization_url")]
        public string AuthorizationUrl { get; set; }

        [JsonPropertyName("access_code")]
        public string AccessCode { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; }
    }

    public class PaymentRequest
    {
        public string Email { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string UserId { get; set; }
        public string PostTitle { get; set; }
    }

    public class PaystackVerifyResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public PaystackVerifyData Data { get; set; }
    }

    public class PaystackVerifyData
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("customer")]
        public PaystackCustomer Customer { get; set; }
    }

    public class PaystackCustomer
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }
    }
}
