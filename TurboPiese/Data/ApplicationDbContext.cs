using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TurboPiese.Models;

namespace TurboPiese.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }

        public DbSet<EditedProduct> EditedProducts { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<EditedPost> EditedPosts { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartPost> CartPosts {  get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CartPost>()
            .HasKey(cp => new {
                cp.Id,
                cp.CartId,
                cp.PostId
            });
            modelBuilder.Entity<CartPost>()
            .HasOne(cp => cp.Cart)
            .WithMany(cp => cp.CartPosts)
            .HasForeignKey(cp => cp.CartId);
            modelBuilder.Entity<CartPost>()
            .HasOne(cp => cp.Post)
            .WithMany(cp => cp.CartPosts)
            .HasForeignKey(cp => cp.PostId);
        }
    }
}
