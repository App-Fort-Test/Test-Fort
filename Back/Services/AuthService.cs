using System.Security.Cryptography;
using System.Text;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        
        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<User?> RegisterAsync(string email, string password, string username)
        {
            // Verificar se email já existe
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return null; // Email já cadastrado
            }
            
            // Verificar se username já existe
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                return null; // Username já cadastrado
            }
            
            // Criar hash da senha
            var passwordHash = HashPassword(password);
            
            // Criar usuário com 10.000 v-bucks iniciais
            var user = new User
            {
                Email = email,
                PasswordHash = passwordHash,
                Username = username,
                Vbucks = 10000,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return user;
        }
        
        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
            {
                return null; // Usuário não encontrado
            }
            
            // Verificar senha
            if (!VerifyPassword(password, user.PasswordHash))
            {
                return null; // Senha incorreta
            }
            
            return user;
        }
        
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }
        
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
        
        private bool VerifyPassword(string password, string passwordHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == passwordHash;
        }
    }
}

