using Microsoft.AspNetCore.Mvc;

namespace GigNovaWebApp.Controllers
{
    public class SellerController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
