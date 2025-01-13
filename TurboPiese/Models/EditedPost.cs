using System.ComponentModel.DataAnnotations;

namespace TurboPiese.Models
{
    public class EditedPost
    {
        [Key]
        public int Id { get; set; }
        public int OriginalPostId { get; set; }
        public int Price { get; set; }

        public string Description { get; set; }
        public string? UserId { get; set; }
        public virtual Post? OriginalPost { get; set; }
        
    }
}
