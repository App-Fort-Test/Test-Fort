using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly UserInventoryService _inventoryService;
        
        public TransactionsController(UserInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetTransactionHistory([FromHeader(Name = "X-User-Id")] string? userIdHeader = null)
        {
            // Tentar obter userId do header (pode vir como string)
            int? userId = null;
            
            // Log para debug
            Console.WriteLine($"GetTransactionHistory chamado - userIdHeader: {userIdHeader}");
            Console.WriteLine($"Request.Headers contém X-User-Id: {Request.Headers.ContainsKey("X-User-Id")}");
            if (Request.Headers.ContainsKey("X-User-Id"))
            {
                Console.WriteLine($"Valor do header X-User-Id: {Request.Headers["X-User-Id"].ToString()}");
            }
            
            if (!string.IsNullOrEmpty(userIdHeader))
            {
                if (int.TryParse(userIdHeader, out int parsedUserId))
                {
                    userId = parsedUserId;
                    Console.WriteLine($"UserId obtido do parâmetro: {userId}");
                }
            }
            
            // Se não conseguiu do header, tentar do Request.Headers diretamente
            if (!userId.HasValue && Request.Headers.TryGetValue("X-User-Id", out var headerValue))
            {
                var headerString = headerValue.ToString().Trim();
                Console.WriteLine($"Tentando obter userId do Request.Headers: '{headerString}'");
                if (!string.IsNullOrEmpty(headerString) && int.TryParse(headerString, out int parsedUserId))
                {
                    userId = parsedUserId;
                    Console.WriteLine($"UserId obtido do Request.Headers: {userId}");
                }
            }
            
            if (!userId.HasValue)
            {
                Console.WriteLine("ERRO: UserId não encontrado no header");
                return Unauthorized(new { message = "É necessário estar logado para ver o histórico de transações" });
            }
            
            Console.WriteLine($"Buscando transações para userId: {userId.Value}");
            var transactions = await _inventoryService.GetTransactionHistoryAsync(userId.Value);
            Console.WriteLine($"Transações encontradas: {transactions.Count} para userId {userId.Value}");
            
            return Ok(new
            {
                transactions = transactions.Select(t => new
                {
                    id = t.Id,
                    cosmeticId = t.CosmeticId,
                    cosmeticName = t.CosmeticName,
                    type = t.Type.ToString(),
                    amount = t.Amount,
                    createdAt = t.CreatedAt
                }).ToList()
            });
        }
        
        [HttpGet("owned")]
        public async Task<IActionResult> GetOwnedCosmetics([FromHeader(Name = "X-User-Id")] string? userIdHeader = null)
        {
            // Tentar obter userId do header
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
                return Unauthorized(new { message = "É necessário estar logado para ver os itens possuídos" });
            }
            
            // Obter itens possuídos baseado nas transações
            var ownedCosmetics = await _inventoryService.GetOwnedCosmeticsFromTransactionsAsync(userId.Value);
            
            return Ok(new
            {
                ownedCosmetics = ownedCosmetics.Select(id => new { cosmeticId = id }).ToList()
            });
        }
        
        [HttpPost("refund/{cosmeticId}")]
        public async Task<IActionResult> RefundCosmetic(string cosmeticId, [FromHeader(Name = "X-User-Id")] string? userIdHeader = null, [FromBody] RefundRequest? request = null)
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
                return Unauthorized(new { message = "É necessário estar logado para devolver cosméticos" });
            }
            
            // Validar cosmeticId
            if (string.IsNullOrWhiteSpace(cosmeticId))
            {
                return BadRequest(new { message = "ID do cosmético é obrigatório" });
            }
            
            var cosmeticName = request?.CosmeticName ?? cosmeticId;
            var success = await _inventoryService.RefundCosmeticAsync(userId.Value, cosmeticId, cosmeticName);
            
            if (!success)
            {
                return BadRequest(new { message = "Não foi possível devolver o cosmético. Verifique se você possui este item." });
            }
            
            var vbucks = await _inventoryService.GetVbucksAsync(userId.Value);
            
            return Ok(new
            {
                success = true,
                vbucks = vbucks,
                message = "Cosmético devolvido com sucesso!"
            });
        }
    }
    
    public class RefundRequest
    {
        public string? CosmeticName { get; set; }
    }
}

