// Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;

namespace TheBrainOfficeServer.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/")]
        public IActionResult Index()
        {
            return File("~/index.html", "text/html");
        }
    }
}