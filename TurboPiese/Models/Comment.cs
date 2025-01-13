using System.ComponentModel.DataAnnotations;

namespace TurboPiese.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Fiecare comentariu trebuie pus la o postare!")]
        public int PostId { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual Post? Post { get; set; }

        [Range(1, 5, ErrorMessage = "Rating-ul trebuie să fie între 1 și 5")]
        public int Rating { get; set; }

        public void UpdatePostRating()
        {
            if (Post != null && Post.Comments != null && Post.Comments.Any())
            {
                Post.Rating = (float)Post.Comments.Average(c => c.Rating);
            }
        }
    }
}
