using ElecShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElecShop.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StoreController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var products = _context.Products.OrderByDescending(p => p.Id).ToList();

            ViewBag.Products = products;

            return View(products);
        }
    }
}
