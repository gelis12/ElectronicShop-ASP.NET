using ElecShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}
