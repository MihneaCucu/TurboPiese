using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TurboPiese.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele piesei este obligatoriu")]
        public string Name { get; set; }

        public int Stock { get; set; }

        public string? Image {  get; set; }
        
        public bool AdminApproved { get; set; }
        [Required(ErrorMessage = "Categoria este obligatorie")]
        public int CategoryId { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public virtual Category? Category { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }


    }
}
