using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class UserCosmetic
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string CosmeticId { get; set; } = "";
        
        public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
        
        public int PurchasePrice { get; set; } // Preço pago (para devolução)
        
        // Navegação
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

