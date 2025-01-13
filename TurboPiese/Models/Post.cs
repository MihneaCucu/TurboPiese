using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurboPiese.Models
{
    public class Post
    {
        internal object data;

        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Postarea trebuie sa aiba pret!")]
        public int Price { get; set; }

        public float Rating { get; set; }

        [Required(ErrorMessage = "Postarea trebuie sa aiba o descriere!")]
        public string Description { get; set; }
        public bool AdminApproved { get; set; }
        [Required(ErrorMessage = "Orice postare trebuie sa aiba un produs!")]
        public int ProductId { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual Product? Product { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? ProductList { get; set; }

        [NotMapped]
        public float AverageRating { get; set; }
        public virtual ICollection<CartPost>? CartPosts
        { get; set; }
    }
}
