using System.Threading.Tasks;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerShop : ControllerBase
    {
        private readonly ShopServices _shopServices;

        public ControllerShop(ShopServices shopServices)
        {
            _shopServices = shopServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetShop()
        {
            var response = await _shopServices.GetShopAsync();
            return Ok(response);
        }
    }
}
