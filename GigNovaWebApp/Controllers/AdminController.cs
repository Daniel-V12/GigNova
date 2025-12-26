using Microsoft.AspNetCore.Mvc;

namespace GigNovaWebApp.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
