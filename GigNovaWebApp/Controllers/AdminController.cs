using Microsoft.AspNetCore.Mvc;
using GigNovaModels.Models;
using GigNovaWSClient;
namespace GigNovaWebApp.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult RemoveGig(string gig_id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult RemoveGigReview(string review_id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddCategory(string category_name, string category_photo)
        {
            return View();
        }

        [HttpPost]
        public IActionResult RemoveCategory(string category_id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult UpdateCategory(string category_id, string category_name, string category_photo)
        {
            return View();
        }

    }
}
