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
                var totalCosmeticsCount = await _enrichedServices.GetTotalCosmeticsCountAsync(userId);
                
                return Ok(new
                {
                    cosmetics = result,
                    totalCount = totalCosmeticsCount,
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
        public async Task<IActionResult> PurchaseCosmetic(string cosmeticId, [FromBody] PurchaseRequest request)
        {
            // Tentar obter userId do header de múltiplas formas
            int? userId = null;
            
            // Método 1: Tentar do Request.Headers diretamente (mais confiável)
            if (Request.Headers.TryGetValue("X-User-Id", out var headerValue))
            {
                var headerString = headerValue.ToString().Trim();
                if (!string.IsNullOrEmpty(headerString) && int.TryParse(headerString, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }
            
            // Método 2: Tentar case-insensitive se não encontrou
            if (!userId.HasValue)
            {
                foreach (var header in Request.Headers)
                {
                    if (header.Key.Equals("X-User-Id", StringComparison.OrdinalIgnoreCase))
                    {
                        var headerString = header.Value.ToString().Trim();
                        if (!string.IsNullOrEmpty(headerString) && int.TryParse(headerString, out int parsedUserId))
                        {
                            userId = parsedUserId;
                            break;
                        }
                    }
                }
            }
            
            // Método 3: Tentar do parâmetro [FromHeader] (pode não funcionar sempre)
            if (!userId.HasValue && Request.Headers.ContainsKey("X-User-Id"))
            {
                var headerValues = Request.Headers["X-User-Id"];
                if (headerValues.Count > 0)
                {
                    var headerString = headerValues[0]?.Trim();
                    if (!string.IsNullOrEmpty(headerString) && int.TryParse(headerString, out int parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }
            }
            
            if (!userId.HasValue)
            {
                return Unauthorized(new { 
                    success = false,
                    message = "É necessário estar logado para comprar cosméticos. UserId não encontrado no header X-User-Id." 
                });
            }
            
            // Validar cosmeticId
            if (string.IsNullOrWhiteSpace(cosmeticId))
            {
                return BadRequest(new { 
                    success = false,
                    message = "ID do cosmético é obrigatório" 
                });
            }
            
            // Validar request
            if (request == null)
            {
                return BadRequest(new { 
                    success = false,
                    message = "Dados da compra são obrigatórios" 
                });
            }
            
            // Validar preço
            if (request.Price <= 0)
            {
                return BadRequest(new { 
                    success = false,
                    message = "Preço deve ser maior que zero" 
                });
            }
            
            try
            {
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
                
                var isOwned = await _inventoryService.IsOwnedAsync(userId.Value, cosmeticId);
                if (isOwned)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Você já possui este cosmético. Cada cosmético só pode ser comprado uma vez." 
                    });
                }
                
                var vbucksCheck = await _inventoryService.GetVbucksAsync(userId.Value);
                if (vbucksCheck < request.Price)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Você não tem créditos suficientes para comprar este cosmético." 
                    });
                }
                
                return BadRequest(new { 
                    success = false, 
                    message = "Não foi possível adquirir o cosmético." 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar compra: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Erro interno ao processar a compra. Tente novamente." 
                });
            }
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory([FromHeader(Name = "X-User-Id")] string? userIdHeader = null)
        {
            // Tentar obter userId do header (pode vir como string)
            int? userId = null;
            if (!string.IsNullOrEmpty(userIdHeader))
            {
                if (int.TryParse(userIdHeader, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }
            
            // Se não conseguiu do header, tentar do Request.Headers diretamente
            if (!userId.HasValue && Request.Headers.TryGetValue("X-User-Id", out var headerValue))
            {
                if (int.TryParse(headerValue.ToString(), out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }
            
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "É necessário estar logado para ver o inventário" });
            }
            
            var ownedCosmetics = await _inventoryService.GetOwnedCosmeticsFromTransactionsAsync(userId.Value);
            var vbucks = await _inventoryService.GetVbucksAsync(userId.Value);
            
            return Ok(new
            {
                ownedCosmetics = ownedCosmetics.Select(id => new { cosmeticId = id }).ToList(),
                vbucks = vbucks
            });
        }

        [HttpGet("vbucks")]
        public async Task<IActionResult> GetVbucks([FromHeader(Name = "X-User-Id")] string? userIdHeader = null)
        {
            // Tentar obter userId do header (pode vir como string)
            int? userId = null;
            if (!string.IsNullOrEmpty(userIdHeader))
            {
                if (int.TryParse(userIdHeader, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }
            
            // Se não conseguiu do header, tentar do Request.Headers diretamente
            if (!userId.HasValue && Request.Headers.TryGetValue("X-User-Id", out var headerValue))
            {
                if (int.TryParse(headerValue.ToString(), out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }
            
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

        [HttpGet("filter-options")]
        public async Task<IActionResult> GetFilterOptions(
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
            [FromHeader(Name = "X-User-Id")] int? userId = null)
        {
            try
            {
                var filterOptions = await _enrichedServices.GetFilterOptionsAsync(
                    name, type, rarity, dateFrom, dateTo, 
                    onlyNew, onlyInShop, onlyOnSale, onlyOwned, onlyBundle,
                    minPrice, maxPrice, userId);
                return Ok(filterOptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar opções de filtros", error = ex.Message });
            }
        }
    }

    public class PurchaseRequest
    {
        public int Price { get; set; }
        public string? CosmeticName { get; set; }
    }
}
