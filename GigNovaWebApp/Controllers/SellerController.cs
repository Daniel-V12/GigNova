using Microsoft.AspNetCore.Mvc;
using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;
using Microsoft.AspNetCore.Http;
namespace GigNovaWebApp.Controllers
{
    public class SellerController : Controller
    {
        [HttpGet]
        public IActionResult HomePage()
        {
            ViewData["HomeActor"] = "seller";
            ViewData["LayoutPath"] = "~/Views/Shared/MasterSellerPage.cshtml";
            return View("~/Views/Shared/HomePage.cshtml");
        }

        [HttpGet]
        public IActionResult SellerHomePage()
        {
            return RedirectToAction("HomePage");
        }

        [HttpGet]
        public async Task<IActionResult> ManageGigs(string seller_id, int page = 0)
        {
            ApiClient<ManageGigsViewModel> client = new ApiClient<ManageGigsViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/GetManageGigsViewModel";
            if (seller_id != null)
            {
                client.AddParameter("seller_id", seller_id);
            }
            if (page != 0)
            {
                client.AddParameter("page", page.ToString());
            }
            ManageGigsViewModel viewModel = await client.GetAsync();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SellerProfile(string seller_id)
        {
            if (seller_id == null || seller_id == "")
            {
                seller_id = HttpContext.Session.GetString("person_id");
            }

            if (seller_id == null || seller_id == "")
            {
                return RedirectToAction("HomePage", "Guest");
            }

            ApiClient<SellerProfileViewModel> client = new ApiClient<SellerProfileViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/GetSellerProfileViewModel";
            client.AddParameter("seller_id", seller_id);

            SellerProfileViewModel viewModel = await client.GetAsync();
            if (viewModel == null)
            {
                viewModel = new SellerProfileViewModel();
            }
            if (viewModel.seller == null)
            {
                viewModel.seller = new Seller();
            }
            if (viewModel.seller_person == null)
            {
                viewModel.seller_person = new Person();
            }

            viewModel.seller.Seller_id = seller_id;
            if (viewModel.seller_person.Person_id == null || viewModel.seller_person.Person_id == "")
            {
                viewModel.seller_person.Person_id = seller_id;
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SellerProfile(SellerProfileViewModel viewModel, IFormFile sellerAvatarFile)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (sellerId == null || sellerId == "")
            {
                return RedirectToAction("HomePage", "Guest");
            }

            Seller sellerToUpdate = new Seller();
            if (viewModel != null && viewModel.seller != null)
            {
                sellerToUpdate = viewModel.seller;
            }

            sellerToUpdate.Seller_id = sellerId;
            if (sellerToUpdate.Seller_display_name == null || sellerToUpdate.Seller_display_name.Trim() == "")
            {
                TempData["SellerProfileMessage"] = "Display name is required.";
                return RedirectToAction("SellerProfile", new { seller_id = sellerId });
            }

            if (sellerToUpdate.Seller_description == null)
            {
                sellerToUpdate.Seller_description = "";
            }

            List<Stream> avatarFiles = new List<Stream>();
            if (sellerAvatarFile != null && sellerAvatarFile.Length > 0)
            {
                avatarFiles.Add(sellerAvatarFile.OpenReadStream());
            }

            ApiClient<Seller> client = new ApiClient<Seller>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Buyer/BecomeASeller";

            bool response = false;
            try
            {
                response = await client.PostAsync(sellerToUpdate, avatarFiles);
            }
            catch
            {
                response = false;
            }

            foreach (Stream stream in avatarFiles)
            {
                stream.Dispose();
            }

            if (response)
            {
                TempData["SellerProfileMessage"] = "Profile updated successfully.";
            }
            else
            {
                TempData["SellerProfileMessage"] = "Failed to update profile.";
            }

            return RedirectToAction("SellerProfile", new { seller_id = sellerId });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (sellerId == null || sellerId == "")
            {
                return RedirectToAction("HomePage", "Guest");
            }

            if (currentPassword == null || currentPassword == "" || newPassword == null || newPassword == "")
            {
                TempData["SellerProfileMessage"] = "Please enter current and new password.";
                return RedirectToAction("SellerProfile", new { seller_id = sellerId });
            }

            ApiClient<string> client = new ApiClient<string>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/ChangeSellerPassword";
            client.AddParameter("seller_id", sellerId);
            client.AddParameter("current_password", currentPassword);
            client.AddParameter("new_password", newPassword);

            bool response = await client.PostAsyncReturn<string, bool>("");
            if (response)
            {
                TempData["SellerProfileMessage"] = "Password changed successfully.";
            }
            else
            {
                TempData["SellerProfileMessage"] = "Failed to change password. Make sure current password is correct.";
            }

            return RedirectToAction("SellerProfile", new { seller_id = sellerId });
        }

        [HttpGet]
        public async Task<IActionResult> Orders(string seller_id)
        {
            ApiClient<OrdersViewModel> client = new ApiClient<OrdersViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/GetOrdersViewModel";
            if (seller_id != null)
            {
                client.AddParameter("seller_id", seller_id);
            }
            OrdersViewModel ordersViewModel = await client.GetAsync();
            return View(ordersViewModel);
        }

        [HttpPost]
        public IActionResult AddGig(Gig gig)
        {
            return View(gig);
        }

        [HttpPost]
        public IActionResult EditGig(Gig gig)
        {
            return View(gig);
        }

        [HttpPost]
        public IActionResult DeleteGig(string seller_id, string gig_id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult PublishGig(string seller_id, string gig_id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult UnpublishGig(string seller_id, string gig_id)
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SelectGig(string gig_id, string seller_id)
        {
            ApiClient<Gig> client = new ApiClient<Gig>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/SelectGig";
            if (gig_id != null)
            {
                client.AddParameter("gig_id", gig_id);
            }
            if (seller_id != null)
            {
                client.AddParameter("seller_id", seller_id);
            }
            Gig gig = await client.GetAsync();
            return View(gig);
        }

        [HttpGet]
        public async Task<IActionResult> SelectOrder(string order_id, string seller_id)
        {
            ApiClient<SelectedOrderViewModel> client = new ApiClient<SelectedOrderViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/SelectOrder";
            if (order_id != null)
            {
                client.AddParameter("order_id", order_id);
            }
            if (seller_id != null)
            {
                client.AddParameter("seller_id", seller_id);
            }
            SelectedOrderViewModel viewModel = await client.GetAsync();
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult DeliverGig(string order_id)
        {
            return View();
        }
    }
}
