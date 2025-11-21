using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
        
        [Required]
        public string PasswordHash { get; set; } = "";
        
        [Required]
        public string Username { get; set; } = "";
        
        public int Vbucks { get; set; } = 10000; // Crédito inicial ao cadastrar
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navegação
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<UserCosmetic> OwnedCosmetics { get; set; } = new List<UserCosmetic>();
    }
}

