using Microsoft.AspNetCore.Mvc;

namespace GigNovaWebApp.Controllers
{
    public class BuyerController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
