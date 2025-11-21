using System.Threading.Tasks;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerCosmeticsNew : ControllerBase
    {
        private readonly CosmeticsNewServices _fortniteNew;

        public ControllerCosmeticsNew(CosmeticsNewServices fortniteNew)
        {
            _fortniteNew = fortniteNew;
        }

        [HttpGet("new")]
        public async Task<IActionResult> GetNewCosmetics()
        {
            var response = await _fortniteNew.GetNewCosmeticsAsync();
            return Ok(response);
        }
    }
}
