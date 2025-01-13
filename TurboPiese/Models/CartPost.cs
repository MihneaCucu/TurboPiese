using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurboPiese.Models
{
    public class CartPost
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? CartId { get; set; }
        public int? PostId { get; set; }
        [Required(ErrorMessage ="Nu puteti selecta 0 bucati")]
        public int Stock {  get; set; }
        public virtual Cart? Cart { get; set; }
        public virtual Post? Post { get; set; }
    }
}
