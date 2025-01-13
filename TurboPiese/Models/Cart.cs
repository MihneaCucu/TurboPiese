using System.ComponentModel.DataAnnotations;

namespace TurboPiese.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<CartPost>? CartPosts
        { get; set; }
    }
}
