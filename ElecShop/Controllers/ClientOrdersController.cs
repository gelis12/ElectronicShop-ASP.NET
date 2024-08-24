using ElecShop.Models;
using ElecShop.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElecShop.Controllers
{
    [Authorize(Roles = "client")]
    [Route("/Client/Orders/{action=Index}/{id?}")]
    public class ClientOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly int _itemsPerPages = 5;

        public ClientOrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(int pageIndex)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null)
            {
                return RedirectToAction("Index", "Home");
            }


            IQueryable<Order> query = _context.Orders.Include(o => o.Items).OrderByDescending(o => o.Id).Where(o => o.ClientId == currentUser.Id);

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / _itemsPerPages);

            query = query.Skip((pageIndex - 1) * _itemsPerPages).Take(_itemsPerPages);


            var orders = query.ToList();



            ViewBag.Orders = orders;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View();
        }
    }
}
