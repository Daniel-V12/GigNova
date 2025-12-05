using Microsoft.AspNetCore.Mvc;

namespace GigNovaWS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SellerController : ControllerBase
    {
        RepositoryUOW repositoryUOW;
        public SellerController()
        {
            this.repositoryUOW = new RepositoryUOW();
        }
    }

   
}