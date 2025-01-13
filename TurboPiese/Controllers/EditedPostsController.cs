using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TurboPiese.Data;
using TurboPiese.Models;

namespace TurboPiese.Controllers
{
    public class EditedPostsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public EditedPostsController(
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
            var editedpost = db.EditedPosts.Find(id);

            if (editedpost == null)
            {
                return NotFound();
            }
            if ((editedpost.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                db.EditedPosts.Remove(editedpost);
                db.SaveChanges();
                TempData["message"] = "Editarea a fost ștearsa cu succes!";
                return RedirectToAction("ApprovalsIndex", "Posts");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("ApprovalsIndex", "Posts");

            }
        }
    }
}
