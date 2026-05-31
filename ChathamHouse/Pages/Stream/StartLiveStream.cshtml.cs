using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChathamHouse.Pages.Stream
{
    public class StartLiveStreamModel : PageModel
    {
        public string StreamName { get; set; } = "ChathamHouse";
        public string StreamManagerUrl { get; set; } = "https://userId-2441-c10d446375.cloud.red5.net";
        public string WhipEndpoint { get; set; } = string.Empty;
        public string WhepEndpoint { get; set; } = string.Empty;

        public void OnGet()
        {
            StreamName = "ChathamHouse";
            StreamManagerUrl = "https://userId-2441-c10d446375.cloud.red5.net";

            // Build the correct endpoints from the documentation
            WhipEndpoint = $"{StreamManagerUrl}/as/v1/proxy/whip/live/{StreamName}";
            WhepEndpoint = $"{StreamManagerUrl}/as/v1/proxy/whep/live/{StreamName}";
        }
    }
}