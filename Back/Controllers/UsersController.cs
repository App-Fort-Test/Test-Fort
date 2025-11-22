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
           
            var usersQuery = _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    id = u.Id,
                    username = u.Username,
                    createdAt = u.CreatedAt
                });
            
            var totalUsersQuery = _context.Users.CountAsync();
            
           
            var usersTask = usersQuery.ToListAsync();
            var totalUsersTask = totalUsersQuery;
            
            await Task.WhenAll(usersTask, totalUsersTask);
            
            var users = await usersTask;
            var totalUsers = await totalUsersTask;
            
          
            var userIds = users.Select(u => u.id).ToList();
            var cosmeticsCounts = await _context.UserCosmetics
                .Where(uc => userIds.Contains(uc.UserId))
                .GroupBy(uc => uc.UserId)
                .Select(g => new { userId = g.Key, count = g.Count() })
                .ToDictionaryAsync(x => x.userId, x => x.count);
            
            var transactionsCounts = await _context.Transactions
                .Where(t => userIds.Contains(t.UserId))
                .GroupBy(t => t.UserId)
                .Select(g => new { userId = g.Key, count = g.Count() })
                .ToDictionaryAsync(x => x.userId, x => x.count);
            
            var usersWithCounts = users.Select(u => new
            {
                id = u.id,
                username = u.username,
                createdAt = u.createdAt,
                totalCosmetics = cosmeticsCounts.ContainsKey(u.id) ? cosmeticsCounts[u.id] : 0,
                totalTransactions = transactionsCounts.ContainsKey(u.id) ? transactionsCounts[u.id] : 0
            }).ToList();
            
            return Ok(new
            {
                users = usersWithCounts,
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

