using Microsoft.AspNetCore.Identity;

namespace TurboPiese.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Post>? Posts { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual Cart? Cart { get; set; }


    }
}
