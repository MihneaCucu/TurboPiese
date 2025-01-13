using TurboPiese.Data;
using TurboPiese.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net.NetworkInformation;

namespace TurboPiese.Controllers
{
    [Authorize(Roles="Editor, Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProductsController(
        ApplicationDbContext context,
        IWebHostEnvironment env,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _env = env;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // [GET] Index - Listarea tuturor produselor
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
            var products = db.Products
                .Include(p => p.Category)
                .Where(p => p.AdminApproved == true)
                .ToList();
            ViewBag.Products = products;  
            return View(products);
        }
        // [GET] Show - Detalii despre un produs
        public IActionResult Show(int id)
        {
            var product = db.Products
                .Include(p => p.Category) // Include informații despre categorie
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            SetAccessRights();
            return View(product);
        }

        // [GET] New - Formular pentru crearea unui produs nou
        public IActionResult New()
        {
            var product = new Product
            {
                Categ = GetAllCategories() // Populează dropdown-ul de categorii
            };

            return View(product);
        }

        // [POST] New - Crearea unui produs nou
        [HttpPost]
        public async Task<IActionResult> New(Product product, IFormFile Image)
        {
            if (Image != null && Image.Length > 0)
            {
                // Verificăm extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(Image.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ArticleImage", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
                    return View(product);
                }
                // Cale stocare
                var storagePath = Path.Combine(_env.WebRootPath, "Images", Image.FileName);
                var databaseFileName = "/Images/" + Image.FileName;
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(product.Image));
                product.Image = databaseFileName;
            }
            if (ModelState.IsValid)
            {
                product.UserId = _userManager.GetUserId(User);
                if (TryValidateModel(product))
                {
                    if (User.IsInRole("Admin"))
                    {
                        product.AdminApproved = true;
                        TempData["message"] = "Produsul a fost creat cu succes!";
                    }
                    else
                    {
                        product.AdminApproved = false;
                        TempData["message"] = "Produsul a fost trimis adminului pentru aprobare!";
                    }
                    db.Products.Add(product);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            product.Categ = GetAllCategories();
            return View(product);
        }

        // [GET] Edit - Formular pentru editarea unui produs
        public IActionResult Edit(int id)
        {
            var product = db.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }
            SetAccessRights();
            if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                product.Categ = GetAllCategories();
                db.SaveChanges();
                return View(product);
            }
            else
            {
                TempData["error_message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // [POST] Edit - Editarea unui produs
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product updatedProduct, IFormFile Image)
        {
            var product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            if (Image != null && Image.Length > 0)
            {
                // Verificăm extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(Image.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ArticleImage", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
                    return View(product);
                }
                // Cale stocare
                var storagePath = Path.Combine(_env.WebRootPath, "Images", Image.FileName);
                var databaseFileName = "/Images/" + Image.FileName;
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(updatedProduct.Image));
                updatedProduct.Image = databaseFileName;
            }
            if (TryValidateModel(updatedProduct))
            {
                if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                {
                    if (User.IsInRole("Admin"))
                    {
                        product.Name = updatedProduct.Name;
                        product.CategoryId = updatedProduct.CategoryId;
                        product.Stock = updatedProduct.Stock;
                        product.Image = updatedProduct.Image;
                        await db.SaveChangesAsync();
                        TempData["message"] = "Produsul a fost actualizat cu succes!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var editedProduct = new EditedProduct();
                        editedProduct.OriginalProductId = product.Id;
                        editedProduct.Name = updatedProduct.Name;
                        editedProduct.UserId = _userManager.GetUserId(User);
                        editedProduct.Stock = updatedProduct.Stock;
                        editedProduct.Image = updatedProduct.Image;
                        db.EditedProducts.Add(editedProduct);
                        await db.SaveChangesAsync();
                        TempData["message"] = "Actualizarea produsului a fost trimisa spre administrator pentru aprobare";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["error_message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                updatedProduct.Categ = GetAllCategories();
                return View(updatedProduct);
            }
        }

        // [POST] Delete - Ștergerea unui produs
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var product = db.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }
            if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin")) {
                if (product.AdminApproved == true)
                {
                    db.Products.Remove(product);
                    db.SaveChanges();
                    TempData["error_message"] = "Produsul a fost șters cu succes!";
                    return RedirectToAction("Index");
                }
                else
                {
                    db.Products.Remove(product);
                    db.SaveChanges();
                    TempData["error_message"] = "Produsul a fost respins!";
                    return RedirectToAction("ApprovalsIndex");

                }
            }
            else
            {
                TempData["error_message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
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
            var products = db.Products
                .Include(pr => pr.Category)
                .Include(pr => pr.User)
                .Where(pr => pr.AdminApproved == false);
            ViewBag.AppProducts = products;
            var editedproducts = db.EditedProducts
                .Include(e => e.OriginalProduct)
                .ThenInclude(p => p.Category);
            ViewBag.EditedProducts = editedproducts;
            return View(products);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Approve(int id)
        {
            var product = db.Products.Find(id);
            if (ModelState.IsValid)
            {
                product.AdminApproved = true;
                db.SaveChanges();
                TempData["message"] = "Produsul a fost aprobat cu succes!";
                return RedirectToAction("ApprovalsIndex");
            }
            return View(product);
        }
        public IActionResult ApproveEdit(int id)
        {
            var editedproduct = db.EditedProducts.Find(id);
            var product = db.Products.Find(editedproduct.OriginalProductId);
            if (ModelState.IsValid)
            {
                product.Name = editedproduct.Name;
                product.UserId = editedproduct.UserId;
                product.Stock = editedproduct.Stock;
                product.Image = editedproduct.Image;
                db.EditedProducts.Remove(editedproduct);
                db.SaveChanges();
                TempData["message"] = "Editarea a fost aprobata cu succes!";
                return RedirectToAction("ApprovalsIndex");
            }
            return View(product);
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

        // Helper pentru dropdown-ul de categorii
        [NonAction]
        private IEnumerable<SelectListItem> GetAllCategories()
        {
            return db.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CategoryName
                })
                .ToList();
        }
    }
}
