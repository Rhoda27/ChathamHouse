using System.Threading.Tasks;
using ChathamHouse.Models;

namespace ChathamHouse.Services
{
    public interface IPaystackService
    {
        Task<PaystackResponse> InitializePayment(PaymentRequest request);
        Task<PaystackVerifyResponse> VerifyPayment(string reference);
    }
}