using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurboPiese.Data;
using TurboPiese.Models;

namespace TurboPiese.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> UserList()
        {
            var users = await _context.Users.ToListAsync();
            var editorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Editor");
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var editors = new List<ApplicationUser>();
            var regularUsers = new List<ApplicationUser>();

            if (editorRole != null)
            {
                var editorRoleId = editorRole.Id;
                editors = await _context.Users
                    .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == editorRoleId))
                    .ToListAsync();
            }

            if (adminRole != null)
            {
                var adminRoleId = adminRole.Id;
                users = users.Where(u => !_context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId)).ToList();
            }

            regularUsers = users.Except(editors).ToList();

            var model = new UserListViewModel
            {
                Editors = editors,
                RegularUsers = regularUsers
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEditorRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Editor");
            }
            return RedirectToAction("UserList");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveEditorRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, "Editor");
            }
            return RedirectToAction("UserList");
        }
    }

    public class UserListViewModel
    {
        public List<ApplicationUser> Editors { get; set; }
        public List<ApplicationUser> RegularUsers { get; set; }
    }
}
