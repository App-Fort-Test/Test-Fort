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
        public async Task<IActionResult> GetTransactionHistory([FromHeader(Name = "X-User-Id")] int? userId)
        {
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }
            
            var transactions = await _inventoryService.GetTransactionHistoryAsync(userId.Value);
            
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
        
        [HttpPost("refund/{cosmeticId}")]
        public async Task<IActionResult> RefundCosmetic(string cosmeticId, [FromHeader(Name = "X-User-Id")] int? userId, [FromBody] RefundRequest? request = null)
        {
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
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

