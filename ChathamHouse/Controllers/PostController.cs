using Microsoft.AspNetCore.Mvc;

namespace ChathamHouse.Controllers
{
    public class PostController : Controller
    {
        public PostController()
        {

        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult index() {
            return View();
        }
        [HttpGet]
        public IActionResult Post()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Verify()
        {
            return View();
        }
    }
}
