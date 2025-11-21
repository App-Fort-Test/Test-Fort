using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CosmeticController : ControllerBase
    {
        private readonly CosmeticsServices _fortnite;

        public CosmeticController(CosmeticsServices fortnite)
        {
            _fortnite = fortnite;
        }

        [HttpGet("cosmetics")]
        public async Task<IActionResult> GetCosmetics()
        {
            var result = await _fortnite.GetCosmeticsAsync();
            if (result == null) return NotFound();
            return Ok(result);
        }

        
    }
}
