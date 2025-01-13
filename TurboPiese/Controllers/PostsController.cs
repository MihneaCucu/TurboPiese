using TurboPiese.Data;
using TurboPiese.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Xml.Linq;

namespace TurboPiese.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public PostsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // [GET] Index - Listarea postărilor
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            if (TempData.ContainsKey("error_message"))
            {
                ViewBag.error_message = TempData["error_message"].ToString();
            }
            var posts = db.Posts
                .Include(p => p.Product)
                .ThenInclude(pr => pr.Category)
                .Where(p => p.AdminApproved == true)
                .ToList();
            var search = "";
            var sortOrder = HttpContext.Request.Query["sortOrder"].ToString();
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 

                // Cautare in articol (Title si Content)

                List<int> postIds = db.Posts.Where
                                        (
                                         at => (at.Product.Name.Contains(search)
                                         || at.Product.Category.CategoryName.Contains(search)
                                        ) && at.AdminApproved == true).Select(a => a.Id).ToList();

                posts = (List<Post>)db.Posts.Where(post => (postIds.Contains(post.Id)) && post.AdminApproved == true)
                                       .Include(p => p.Product)
                                        .ThenInclude(pr => pr.Category)

                                      .OrderBy(a => a.Price)
                                        .ToList();
            }

            // Sortare
            switch (sortOrder)
            {
                case "price_asc":
                    posts = posts.OrderBy(a => a.Price).ToList();
                    break;
                case "price_desc":
                    posts = posts.OrderByDescending(a => a.Price).ToList();
                    break;
                case "rating_asc":
                    posts = posts.OrderBy(a => a.Rating).ToList();
                    break;
                case "rating_desc":
                    posts = posts.OrderByDescending(a => a.Rating).ToList();
                    break;
                default:
                    posts = posts.OrderBy(a => a.Price).ToList();
                    break;
            }
            ViewBag.SearchString = search;
            ViewBag.Posts = posts;
            //Afisare paginata

            int _perPage = 12;

            int totalItems = posts.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedPosts = posts.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            ViewBag.Posts = paginatedPosts;


            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Posts/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Posts/Index/?page";
            }

            return View();
        }

        // [GET] Show - Detalii pentru o postare
        public IActionResult Show(int id)
        {

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            if (TempData.ContainsKey("error_message"))
            {
                ViewBag.error_message = TempData["error_message"].ToString();
            }
            var post = db.Posts
                .Include(p => p.Comments)
                .Include(p => p.Product)
                .ThenInclude(pr => pr.Category)
                .FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            UpdatePostRating(post);
            db.SaveChanges();

            SetAccessRights();
            return View(post);
        }
        [Authorize(Roles ="Admin, Editor, User")]
        [HttpPost]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();

                var post = db.Posts.Include(p => p.Comments).FirstOrDefault(p => p.Id == comment.PostId);
                if (post != null)
                {
                    post.Rating = (float)post.Comments.Average(c => c.Rating);
                    db.SaveChanges();
                }

                SetAccessRights();
                return Redirect("/Posts/Show/" + comment.PostId);
            }
            else
            {
                Post post = db.Posts.Include("Product")
                                         .Include("Comments")
                                         .Where(post => post.Id == comment.PostId)
                                         .First();
                SetAccessRights();
                return View(post);
            }
        }
        [Authorize (Roles ="Admin, Editor")]
        // [GET] New - Formular pentru o postare nouă
        public IActionResult New()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            if (TempData.ContainsKey("error_message"))
            {
                ViewBag.error_message = TempData["error_message"].ToString();
            }
            var post = new Post
            {
                ProductList = GetAllProducts()
            };

            return View(post);
        }
        [Authorize(Roles = "Admin, Editor")]
        // [POST] New - Crearea unei postări noi
        [HttpPost]
        public IActionResult New(Post post)
        {
            post.data = DateTime.Now;
            post.UserId = _userManager.GetUserId(User);
            var prods = db.Posts.Select(p => p.ProductId).ToList();
            if (prods.Contains(post.ProductId))
            {
                TempData["error_message"] = "Nu puteti crea mai multe postari pentru acelasi produs!";
                post.ProductList = GetAllProducts();
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    post.AdminApproved = true;
                    TempData["message"] = "Postarea a fost creată cu succes!";
                }
                else
                {
                    post.AdminApproved = false;
                    TempData["message"] = "Postarea a fost trimisa adminului pentru aprobare!";
                }
                db.Posts.Add(post);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            post.ProductList = GetAllProducts();
            return View(post);
        }

        // [GET] Edit - Formular pentru editarea unei postări
        [Authorize(Roles = "Admin, Editor")]
        public IActionResult Edit(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            if (TempData.ContainsKey("error_message"))
            {
                ViewBag.error_message = TempData["error_message"].ToString();
            }
            var post = db.Posts.Find(id);

            if (post == null)
            {
                return NotFound();
            }
            SetAccessRights();
            post.ProductList = GetAllProducts();
            if ((post.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                return View(post);
            else
            {
                TempData["error_message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // [POST] Edit - Editarea unei postări
        [HttpPost]
        [Authorize(Roles = "Admin, Editor")]
        public IActionResult Edit(int id, Post updatedPost)
        {
            var post = db.Posts.Find(id);

            if (post == null)
            {
                return NotFound();
            }
            var prods = db.Posts.Where(p => p.Id != id).Select(p => p.ProductId).ToList();
            if (prods.Contains(updatedPost.ProductId))
            {
                TempData["error_message"] = "Nu puteti crea mai multe postari pentru acelasi produs!";
                post.ProductList = GetAllProducts();
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                if ((post.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                {
                    if (User.IsInRole("Admin"))
                    {
                        post.Price = updatedPost.Price;
                        post.ProductId = updatedPost.ProductId;
                        post.Description = updatedPost.Description;
                        db.SaveChanges();
                        TempData["message"] = "Postarea a fost actualizată cu succes!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var editedPost = new EditedPost();
                        editedPost.OriginalPostId = post.Id;
                        editedPost.Price = updatedPost.Price;
                        editedPost.Description = updatedPost.Description;
                        editedPost.UserId = _userManager.GetUserId(User);
                        db.EditedPosts.Add(editedPost);
                        db.SaveChanges();
                        TempData["message"] = "Actualizarea postarii a fost trimisa spre administrator pentru aprobare";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["error_message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");

                }
            }
            else
            {
                updatedPost.ProductList = GetAllProducts();
                return View(updatedPost);
            }
        }
            [HttpPost]
        [Authorize(Roles = "Admin, Editor")]
        public IActionResult Delete(int id)
        {
            var post = db.Posts.Find(id);

            if (post == null)
            {
                return NotFound();
            }
            if ((post.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                if (post.AdminApproved == true)
                {
                    db.Posts.Remove(post);
                    db.SaveChanges();
                    TempData["error_message"] = "Postarea a fost ștearsa cu succes!";
                    return RedirectToAction("Index");
                }
                else
                {
                    db.Posts.Remove(post);
                    db.SaveChanges();
                    TempData["error_message"] = "Postarea a fost respinsa!";
                    return RedirectToAction("ApprovalsIndex");

                }
            }
            else
            {
                TempData["error_message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");

            }
        }

        public IActionResult ApprovalsIndex()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            if (TempData.ContainsKey("error_message"))
            {
                ViewBag.error_message = TempData["error_message"].ToString();
            }
            var posts = db.Posts
                .Include(p => p.Product)
                .ThenInclude(pr => pr.Category)
                .Include(pr => pr.User)
                .Where(p => p.AdminApproved == false);
            ViewBag.AppPosts = posts;
            var editedposts = db.EditedPosts
                .Include(e => e.OriginalPost)
                .ThenInclude(p => p.Product)
                .ThenInclude(pr => pr.Category);
            ViewBag.EditedPosts = editedposts;
            return View(posts);
        }
        [HttpPost]
        [Authorize(Roles ="Admin")]
        public IActionResult Approve (int id)
        {
            var post = db.Posts.Find(id);
            if (ModelState.IsValid)
            {
                post.AdminApproved = true;
                db.SaveChanges();
                TempData["message"] = "Postarea a fost aprobata cu succes!";
                return RedirectToAction("ApprovalsIndex");
            }
            return View(post);
        }

        public IActionResult ApproveEdit(int id)
        {
            var editedpost = db.EditedPosts.Find(id);
            var post = db.Posts.Find(editedpost.OriginalPostId);
            if (ModelState.IsValid)
            {
                post.Price = editedpost.Price;
                post.UserId = editedpost.UserId;
                post.Description = editedpost.Description;
                db.EditedPosts.Remove(editedpost);
                db.SaveChanges();
                TempData["message"] = "Editarea a fost aprobata cu succes!";
                return RedirectToAction("ApprovalsIndex");
            }
            return View(post);
        }
        [Authorize(Roles = "Admin, Editor, User")]
        public IActionResult AddToCart([FromForm] CartPost cartpost)
        {
            if(db.Carts.Where(c => c.UserId == _userManager.GetUserId(User)).Count() == 0)
            {
                var cart = new Cart();
                cart.UserId = _userManager.GetUserId(User);
                db.Carts.Add(cart);
                db.SaveChanges();
            }
            cartpost.CartId = db.Carts.FirstOrDefault(c => c.UserId == _userManager.GetUserId(User)).Id;
            if (ModelState.IsValid)
            { 
                if (db.CartPosts
                    .Where(cp => cp.CartId == cartpost.CartId)
                    .Where(cp => cp.PostId == cartpost.PostId)
                    .Count() > 0)
                {
                    TempData["error_message"] = "Acest produs a mai fost adaugat o data in cos";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {
                    var post = db.Posts.Find(cartpost.PostId);
                    var product = db.Products.Find(post.ProductId);
                    if (cartpost.Stock > product.Stock)
                    {
                        TempData["error_message"] = "Nu mai sunt suficiente produse in stoc pentru a finaliza comanda!";
                        TempData["messageType"] = "alert-danger";
                        return RedirectToAction("Index");
                    }
                    db.CartPosts.Add(cartpost);
                    // Salvam modificarile
                    db.SaveChanges();
                    // Adaugam un mesaj de succes
                    TempData["message"] = "Produsul a fost adaugat cu succes in cos";
                    TempData["messageType"] = "alert-success";
                }

            }
            else
            {
                TempData["error_message"] = "Nu s-a putut adauga produsul in cos";
                TempData["messageType"] = "alert-danger";
            }

            // Ne intoarcem la pagina articolului
            return Redirect("/Posts/Show/" + cartpost.PostId);
        }
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Editor"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }

        // Helper pentru dropdown-ul de produse
        [NonAction]
        private IEnumerable<SelectListItem> GetAllProducts()
        {
            return db.Products

                .Where(p => p.AdminApproved == true)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} ({p.Category.CategoryName}) stoc: {p.Stock} bucati"
                })
               
                .ToList();
        }

        public void UpdatePostRating(Post post)
        {
            IEnumerable<Comment> Comments;
            Comments = post.Comments;
            if (Comments != null && Comments.Any())
            {
                post.Rating = (float)Comments.Average(c => c.Rating);
            }
            else
            {
                post.Rating = 0;
            }
        }
    }
}
