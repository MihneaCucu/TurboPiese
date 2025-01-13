using Microsoft.Build.ObjectModelRemoting;
using System.ComponentModel.DataAnnotations;

namespace TurboPiese.Models
{
    public class EditedProduct
    {
        [Key]
        public int Id { get; set; }
        public int OriginalProductId { get; set; }
        public string Name { get; set; }

        public int Stock {  get; set; }

        public string? Image { get; set; }
        public string? UserId { get; set; }
        public virtual Product? OriginalProduct { get; set; }
    }
}
