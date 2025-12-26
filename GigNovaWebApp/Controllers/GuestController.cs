using Microsoft.AspNetCore.Mvc;
using GigNovaWSClient;
using GigNovaModels;
using GigNovaModels.ViewModels;
namespace GigNovaWebApp.Controllers
{
    public class GuestController : Controller
    {
        [HttpGet]
        public IActionResult HomePage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ViewCatalog(string category_id = null, int page = 0)
        {
            ApiClient<CatalogViewModel> client = new ApiClient<CatalogViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Guest/GetCatalogViewModel";
            if (category_id != null)
            {
                client.AddParameter("category_id", category_id);
            }
            if (page != 0 )
            {
                client.AddParameter("page", page.ToString());
            }
            CatalogViewModel catalogViewModel = await client.GetAsync();
            return View(catalogViewModel);
        }

        public async Task<IActionResult> ViewSelectedGig (string gig_id = null)
        {
            ApiClient<SelectedGigViewModel> client = new ApiClient<SelectedGigViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 5172;
            client.Path = "api/Guest/GetSelectedGigViewModel/";
            if (gig_id != null)
            {
                client.AddParameter("gig_id", gig_id);
            }
            SelectedGigViewModel selectedGigViewModel = await client.GetAsync();
            return View(selectedGigViewModel);
        }

    }
}
