using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TurboPiese.Models;

namespace TurboPiese.Views.Users
{
    public class UserListModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserListModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<ApplicationUser> Users { get; set; }
        public List<ApplicationUser> Editors { get; set; }
        public UserManager<ApplicationUser> UserManager => _userManager;

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToPage("/Index");
            }

            Users = _userManager.Users.ToList();
            var editorRole = await _roleManager.FindByNameAsync("Editor");
            if (editorRole != null)
            {
                Editors = (await _userManager.GetUsersInRoleAsync("Editor")).ToList();
            }
            else
            {
                Editors = new List<ApplicationUser>();
            }

            return Page();
        }
    }
}
