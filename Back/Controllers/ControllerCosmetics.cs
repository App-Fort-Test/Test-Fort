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
    public class ControllerCosmetics : ControllerBase
    {
        private readonly CosmeticsServices _fortnite;

        public ControllerCosmetics(CosmeticsServices fortnite)
        {
            _fortnite = fortnite;
        }

        [HttpGet("cosmetics")]
        public async Task<IActionResult> GetCosmetics([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            // Limita os valores permitidos
            int[] allowedSizes = { 10, 20, 50, 100 };
            if (!allowedSizes.Contains(pageSize)) pageSize = 50;

            var items = await _fortnite.GetBrCosmeticsPagedAsync(page, pageSize);
            return Ok(items);
        }
    }
}
