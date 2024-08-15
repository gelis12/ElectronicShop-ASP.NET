using ElecShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElecShop.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly int _usersPerPage = 5;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        

        public IActionResult Index(int? pageIndex)
        {
            IQueryable<ApplicationUser> query = _userManager.Users.OrderByDescending(u => u.CreatedAt);

            if (pageIndex is null || pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / _usersPerPage);

            query = query.Skip(((int)pageIndex - 1) * _usersPerPage).Take(_usersPerPage);

            var users = query.ToList();

            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;


            return View(users);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id is null)
            {
                return RedirectToAction("Index", "Users");
            }

            var appUsers = await _userManager.FindByIdAsync(id);

            if (appUsers is null)
            {
                return RedirectToAction("Index", "Users");
            }

            ViewBag.Roles = await _userManager.GetRolesAsync(appUsers);


            var availableRoles = _roleManager.Roles.ToList();
            var items = new List<SelectListItem>();

            foreach (var role in availableRoles)
            {
                items.Add(
                    new SelectListItem
                    {
                        Text = role.NormalizedName,
                        Value = role.Name,
                        Selected = await _userManager.IsInRoleAsync(appUsers, role.Name!)
                    }
                );
            }

            ViewBag.SelectItems = items;

            return View(appUsers);
        }

        public async Task<IActionResult> EditRole(string? id, string? newRole)
        {
            if (id is null || newRole is null)
            {
                return RedirectToAction("Index", "Users");
            }

            var roleExists = await _roleManager.RoleExistsAsync(newRole);
            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser is null || roleExists is false)
            {
                return RedirectToAction("Index", "Users");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser!.Id == appUser.Id)
            {
                TempData["ErrorMessage"] = "You cannot update your own role!";
                return RedirectToAction("Details", "Users", new { id });
            }

            var userRoles = await _userManager.GetRolesAsync(appUser);
            await _userManager.RemoveFromRolesAsync(appUser, userRoles);
            await _userManager.AddToRoleAsync(appUser, newRole);

            TempData["SuccessMessage"] = "User Role updated successfully!";

            return RedirectToAction("Details", "Users", new { id });


        }





    }
}
