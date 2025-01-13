using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TurboPiese.Data;
using TurboPiese.Models;

namespace TurboPiese.Controllers
{
    public class EditedProductsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public EditedProductsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Delete(int id)
        {
            var editedproduct = db.EditedProducts.Find(id);

            if (editedproduct == null)
            {
                return NotFound();
            }
            if ((editedproduct.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                db.EditedProducts.Remove(editedproduct);
                db.SaveChanges();
                TempData["message"] = "Editarea a fost ștearsa cu succes!";
                return RedirectToAction("ApprovalsIndex", "Products");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("ApprovalsIndex", "Products");

            }
        }
    }
}
