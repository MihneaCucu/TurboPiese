using TurboPiese.Data;
using TurboPiese.Models;
using Microsoft.AspNetCore.Mvc;
using TurboPiese.Data;
using TurboPiese.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;

namespace TurboPiese.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CommentsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // Stergerea unui comentariu asociat unui articol din baza de date
        [HttpPost]
        [Authorize(Roles = "Admin, Editor, User")]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);
            if ((comm.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();

                comm.UpdatePostRating();
                TempData["error_message"] = "Comentariul a fost sters";
                db.SaveChanges();
                return Redirect("/Posts/Show/" + comm.PostId);
            }
            else
            {
                TempData["error_message"] = "Nu aveti dreptul sa stergeti un comentariu care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("/Posts/Show/" + comm.PostId);
            }
        }

        // In acest moment vom implementa editarea intr-o pagina View separata
        // Se editeaza un comentariu existent

        [Authorize(Roles = "Admin, Editor, User")]
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);
            if ((comm.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                return View(comm);
            }
            else
            {
                TempData["error_message"] = "Nu aveti dreptul sa stergeti un comentariu care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("/Posts/Show/" + comm.PostId);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Editor, User")]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    comm.Content = requestComment.Content;
                    comm.Rating = requestComment.Rating;

                    db.SaveChanges();

                    return Redirect("/Posts/Show/" + comm.PostId);
                }
                else
                {
                    return View(requestComment);
                }
            }
            else
            {
                TempData["error_message"] = "Nu aveti dreptul sa editati comentariul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Posts");
            }

        }
    }
}