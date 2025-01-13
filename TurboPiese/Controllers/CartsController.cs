using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using TurboPiese.Data;
using TurboPiese.Models;

namespace TurboPiese.Controllers
{
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CartsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Admin, Editor, User")]
        public IActionResult Show()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            if (TempData.ContainsKey("error_message"))
            {
                ViewBag.error_message = TempData["error_message"].ToString();
            }
            if (db.Carts.Where(c => c.UserId == _userManager.GetUserId(User)).Count() == 0)
            {
                var usercart = new Cart();
                usercart.UserId = _userManager.GetUserId(User);
                db.Carts.Add(usercart);
                db.SaveChanges();
            }
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            if (TempData.ContainsKey("error_message"))
            {
                ViewBag.error_message = TempData["error_message"].ToString();
            }
            var cart = db.Carts
                .Where(c => c.UserId == _userManager.GetUserId(User))
                .Include(c => c.CartPosts)
                .ThenInclude(cp => cp.Post)
                .ThenInclude(p => p.Product)
                .ThenInclude(pr => pr.Category)
                .FirstOrDefault(p => p.UserId == _userManager.GetUserId(User));
            if (cart == null)
            {
                return NotFound();
            }
            int pretTotal = 0;
            IEnumerable<CartPost> cartposts = db.CartPosts.Where(cp => cp.CartId == cart.Id);
            foreach(var cartpost in cartposts)
            {
                pretTotal += cartpost.Post.Price * cartpost.Stock;
            }
            ViewBag.PretTotal = pretTotal;
            return View(cart);
        }
        public IActionResult DeleteProduct(int id)
        {
            var post = db.Posts.Find(id);
            if (post == null)
            {
                return NotFound();
            }
            var cartpost = db.CartPosts.Where(cp => cp.PostId == post.Id 
                                              && cp.Cart.UserId == _userManager.GetUserId(User)).FirstOrDefault();
            if (cartpost == null)
            {
                return NotFound();
            }
            var product = db.Products.Find(post.ProductId);
            product.Stock += cartpost.Stock;
            db.CartPosts.Remove(cartpost);
            db.SaveChanges();
            TempData["error_message"] = "Produsul a fost eliminat din cos!";
            return RedirectToAction("Show");
        }
        public IActionResult Order(int id)
        {
            var cart = db.Carts.Find(id);
            IEnumerable<CartPost> cartposts = db.CartPosts.Where(cp => cp.CartId == id);
            if(cartposts.Count() == 0)
            {
                TempData["message"] = "Cosul este gol!";
                return RedirectToAction("Show");
            }
            foreach(var cp in cartposts)
            {
                var product = db.Products.Find(db.Posts.Find(cp.PostId).ProductId);
                product.Stock -= cp.Stock;
                if(product.Stock < 0)
                {
                    product.Stock = 0;
                }
                db.CartPosts.Remove(cp);
                
            }
            db.SaveChanges();
            TempData["message"] = "Comanda a fost plasata cu succes!";
            return RedirectToAction("Show");
        }
    }
    
}
