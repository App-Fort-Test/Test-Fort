using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    id = u.Id,
                    username = u.Username,
                    createdAt = u.CreatedAt,
                    totalCosmetics = u.OwnedCosmetics.Count
                })
                .ToListAsync();
                
            var totalUsers = await _context.Users.CountAsync();
            
            return Ok(new
            {
                users = users,
                totalCount = totalUsers,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize)
            });
        }
        
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            var user = await _context.Users
                .Include(u => u.OwnedCosmetics)
                .FirstOrDefaultAsync(u => u.Id == userId);
                
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }
            
            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                createdAt = user.CreatedAt,
                totalCosmetics = user.OwnedCosmetics.Count,
                ownedCosmetics = user.OwnedCosmetics.Select(uc => new
                {
                    cosmeticId = uc.CosmeticId,
                    acquiredAt = uc.AcquiredAt
                }).ToList()
            });
        }
        
        [HttpGet("{userId}/cosmetics")]
        public async Task<IActionResult> GetUserCosmetics(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }
            
            var cosmetics = await _context.UserCosmetics
                .Where(uc => uc.UserId == userId)
                .OrderByDescending(uc => uc.AcquiredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(uc => new
                {
                    cosmeticId = uc.CosmeticId,
                    acquiredAt = uc.AcquiredAt,
                    purchasePrice = uc.PurchasePrice
                })
                .ToListAsync();
                
            var totalCosmetics = await _context.UserCosmetics
                .CountAsync(uc => uc.UserId == userId);
            
            return Ok(new
            {
                cosmetics = cosmetics,
                totalCount = totalCosmetics,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(totalCosmetics / (double)pageSize)
            });
        }
    }
}

