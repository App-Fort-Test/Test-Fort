using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class UserInventoryService
    {
        private readonly ApplicationDbContext _context;
        
        public UserInventoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<bool> IsOwnedAsync(int userId, string cosmeticId)
        {
            return await _context.UserCosmetics
                .AnyAsync(uc => uc.UserId == userId && uc.CosmeticId == cosmeticId);
        }
        
        public async Task<List<string>> GetOwnedCosmeticsAsync(int userId)
        {
            return await _context.UserCosmetics
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CosmeticId)
                .ToListAsync();
        }
        
        public async Task<int> GetVbucksAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.Vbucks ?? 0;
        }
        
        public async Task<bool> PurchaseCosmeticAsync(int userId, string cosmeticId, string cosmeticName, int price)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            
            // Verificar se já possui o cosmético
            if (await IsOwnedAsync(userId, cosmeticId))
            {
                return false;
            }
            
            // Verificar se tem créditos suficientes
            if (user.Vbucks < price)
            {
                return false;
            }
            
            // Debitar créditos
            user.Vbucks -= price;
            
            // Adicionar cosmético ao inventário
            var userCosmetic = new UserCosmetic
            {
                UserId = userId,
                CosmeticId = cosmeticId,
                PurchasePrice = price,
                AcquiredAt = DateTime.UtcNow
            };
            _context.UserCosmetics.Add(userCosmetic);
            
            // Registrar transação
            var transaction = new Transaction
            {
                UserId = userId,
                CosmeticId = cosmeticId,
                CosmeticName = cosmeticName,
                Type = TransactionType.Purchase,
                Amount = -price, // Negativo para compra (debita)
                CreatedAt = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> RefundCosmeticAsync(int userId, string cosmeticId, string cosmeticName)
        {
            var userCosmetic = await _context.UserCosmetics
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CosmeticId == cosmeticId);
                
            if (userCosmetic == null)
            {
                return false; // Cosmético não encontrado no inventário
            }
            
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            
            // Reembolsar créditos
            user.Vbucks += userCosmetic.PurchasePrice;
            
            // Remover cosmético do inventário
            _context.UserCosmetics.Remove(userCosmetic);
            
            // Registrar transação de devolução
            var transaction = new Transaction
            {
                UserId = userId,
                CosmeticId = cosmeticId,
                CosmeticName = cosmeticName,
                Type = TransactionType.Refund,
                Amount = userCosmetic.PurchasePrice, // Positivo para devolução (credita)
                CreatedAt = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<List<Transaction>> GetTransactionHistoryAsync(int userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<bool> PurchaseBundleAsync(int userId, List<(string CosmeticId, string CosmeticName, int Price)> cosmetics)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            
            // Calcular preço total
            var totalPrice = cosmetics.Sum(c => c.Price);
            
            // Verificar se tem créditos suficientes
            if (user.Vbucks < totalPrice)
            {
                return false;
            }
            
            // Verificar se já possui algum dos cosméticos
            var cosmeticIds = cosmetics.Select(c => c.CosmeticId).ToList();
            var ownedCosmetics = await _context.UserCosmetics
                .Where(uc => uc.UserId == userId && cosmeticIds.Contains(uc.CosmeticId))
                .Select(uc => uc.CosmeticId)
                .ToListAsync();
                
            if (ownedCosmetics.Any())
            {
                return false; // Já possui algum cosmético do bundle
            }
            
            // Debitar créditos
            user.Vbucks -= totalPrice;
            
            // Adicionar todos os cosméticos ao inventário
            foreach (var cosmetic in cosmetics)
            {
                var userCosmetic = new UserCosmetic
                {
                    UserId = userId,
                    CosmeticId = cosmetic.CosmeticId,
                    PurchasePrice = cosmetic.Price,
                    AcquiredAt = DateTime.UtcNow
                };
                _context.UserCosmetics.Add(userCosmetic);
                
                // Registrar transação para cada cosmético
                var transaction = new Transaction
                {
                    UserId = userId,
                    CosmeticId = cosmetic.CosmeticId,
                    CosmeticName = cosmetic.CosmeticName,
                    Type = TransactionType.Purchase,
                    Amount = -cosmetic.Price,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Transactions.Add(transaction);
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
