using Microsoft.AspNetCore.Mvc;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BundlesController : ControllerBase
    {
        private readonly UserInventoryService _inventoryService;
        
        public BundlesController(UserInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }
        
        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseBundle([FromBody] PurchaseBundleRequest request, [FromHeader(Name = "X-User-Id")] int? userId)
        {
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "É necessário estar logado para comprar bundles" });
            }
            
            if (request.Cosmetics == null || request.Cosmetics.Count == 0)
            {
                return BadRequest(new { message = "Bundle deve conter pelo menos um cosmético" });
            }
            
            var cosmetics = request.Cosmetics.Select(c => (CosmeticId: c.CosmeticId, CosmeticName: c.CosmeticName, Price: c.Price)).ToList();
            var success = await _inventoryService.PurchaseBundleAsync(userId.Value, cosmetics);
            
            if (success)
            {
                var vbucks = await _inventoryService.GetVbucksAsync(userId.Value);
                return Ok(new
                {
                    success = true,
                    vbucks = vbucks,
                    message = "Bundle adquirido com sucesso!"
                });
            }
            
            return BadRequest(new
            {
                success = false,
                message = "Não foi possível adquirir o bundle. Verifique se você tem créditos suficientes ou se já possui algum item do bundle."
            });
        }
    }
    
    public class PurchaseBundleRequest
    {
        public List<BundleCosmetic> Cosmetics { get; set; } = new();
    }
    
    public class BundleCosmetic
    {
        public string CosmeticId { get; set; } = "";
        public string CosmeticName { get; set; } = "";
        public int Price { get; set; }
    }
}

