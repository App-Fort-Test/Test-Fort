using System;
using System.Threading.Tasks;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerCosmeticsEnriched : ControllerBase
    {
        private readonly CosmeticsEnrichedServices _enrichedServices;
        private readonly UserInventoryService _inventoryService;

        public ControllerCosmeticsEnriched(
            CosmeticsEnrichedServices enrichedServices,
            UserInventoryService inventoryService)
        {
            _enrichedServices = enrichedServices;
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCosmetics(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string sortBy = "",
            [FromHeader(Name = "X-User-Id")] int? userId = null)
        {
            var cosmetics = await _enrichedServices.GetEnrichedCosmeticsAsync(page, pageSize, sortBy, userId);
            return Ok(cosmetics);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchCosmetics(
            [FromQuery] string name = "",
            [FromQuery] string type = "",
            [FromQuery] string rarity = "",
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
                    [FromQuery] bool? onlyNew = null,
                    [FromQuery] bool? onlyInShop = null,
                    [FromQuery] bool? onlyOnSale = null,
                    [FromQuery] bool? onlyOwned = null,
                    [FromQuery] bool? onlyBundle = null,
            [FromQuery] int? minPrice = null,
            [FromQuery] int? maxPrice = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
                    [FromQuery] string sortBy = "",
                    [FromHeader(Name = "X-User-Id")] int? userId = null)
        {
            // Se não há filtros ativos, usa o método mais rápido
                    bool hasFilters = !string.IsNullOrWhiteSpace(name) ||
                                      !string.IsNullOrWhiteSpace(type) ||
                                      !string.IsNullOrWhiteSpace(rarity) ||
                                      dateFrom.HasValue ||
                                      dateTo.HasValue ||
                                      onlyNew == true ||
                                      onlyInShop == true ||
                                      onlyOnSale == true ||
                                      onlyOwned == true ||
                                      onlyBundle == true ||
                                      minPrice.HasValue ||
                                      maxPrice.HasValue;

            if (!hasFilters)
            {
                // Sem filtros, usa método paginado direto (mais rápido)
                var result = await _enrichedServices.GetEnrichedCosmeticsAsync(page, pageSize, sortBy, userId);
                // Obter contagem total de cosméticos de forma otimizada (com cache)
                var totalCount = await _enrichedServices.GetTotalCosmeticsCountAsync(userId);
                
                return Ok(new
                {
                    cosmetics = result,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize
                });
            }

            // Com filtros, usa busca completa
            var filter = new CosmeticSearchFilter
            {
                Name = name,
                Type = type,
                Rarity = rarity,
                DateFrom = dateFrom,
                DateTo = dateTo,
                        OnlyNew = onlyNew,
                        OnlyInShop = onlyInShop,
                        OnlyOnSale = onlyOnSale,
                        OnlyOwned = onlyOwned,
                        OnlyBundle = onlyBundle,
                        MinPrice = minPrice,
                        MaxPrice = maxPrice,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy
            };

            var cosmetics = await _enrichedServices.SearchCosmeticsAsync(filter, userId);
            var totalCount = await _enrichedServices.GetTotalCountAsync(filter, userId);
            
            return Ok(new
            {
                cosmetics = cosmetics,
                totalCount = totalCount,
                page = page,
                pageSize = pageSize
            });
        }

        [HttpPost("purchase/{cosmeticId}")]
        public async Task<IActionResult> PurchaseCosmetic(string cosmeticId, [FromBody] PurchaseRequest request, [FromHeader(Name = "X-User-Id")] int? userId)
        {
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "É necessário estar logado para comprar cosméticos" });
            }
            
            var cosmeticName = request.CosmeticName ?? cosmeticId;
            var success = await _inventoryService.PurchaseCosmeticAsync(userId.Value, cosmeticId, cosmeticName, request.Price);
            
            if (success)
            {
                var vbucks = await _inventoryService.GetVbucksAsync(userId.Value);
                return Ok(new { 
                    success = true, 
                    vbucks = vbucks,
                    message = "Cosmético adquirido com sucesso!" 
                });
            }
            
            return BadRequest(new { 
                success = false, 
                message = "Não foi possível adquirir o cosmético. Verifique se você tem créditos suficientes ou se já possui este item." 
            });
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory([FromHeader(Name = "X-User-Id")] int? userId)
        {
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "É necessário estar logado para ver o inventário" });
            }
            
            var ownedCosmetics = await _inventoryService.GetOwnedCosmeticsAsync(userId.Value);
            var vbucks = await _inventoryService.GetVbucksAsync(userId.Value);
            
            return Ok(new
            {
                ownedCosmetics = ownedCosmetics,
                vbucks = vbucks
            });
        }

        [HttpGet("vbucks")]
        public async Task<IActionResult> GetVbucks([FromHeader(Name = "X-User-Id")] int? userId)
        {
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "É necessário estar logado para ver os créditos" });
            }
            
            var vbucks = await _inventoryService.GetVbucksAsync(userId.Value);
            return Ok(new { vbucks = vbucks });
        }

        [HttpGet("new")]
        public async Task<IActionResult> GetNewCosmetics()
        {
            try
            {
                var newCosmetics = await _enrichedServices.GetNewCosmeticsDataAsync();
                return Ok(newCosmetics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar novos cosméticos", error = ex.Message });
            }
        }

        [HttpGet("shop")]
        public async Task<IActionResult> GetShop()
        {
            try
            {
                var shop = await _enrichedServices.GetShopDataAsync();
                return Ok(shop);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar shop", error = ex.Message });
            }
        }
    }

    public class PurchaseRequest
    {
        public int Price { get; set; }
        public string? CosmeticName { get; set; }
    }
}
