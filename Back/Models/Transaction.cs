using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string CosmeticId { get; set; } = "";
        
        [Required]
        public string CosmeticName { get; set; } = "";
        
        [Required]
        public TransactionType Type { get; set; }
        
        [Required]
        public int Amount { get; set; } // Valor em v-bucks (positivo para compra, negativo para devolução)
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navegação
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
    
    public enum TransactionType
    {
        Purchase = 1,
        Refund = 2
    }
}

