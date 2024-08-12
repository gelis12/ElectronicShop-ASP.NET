using ElecShop.Models;
using ElecShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElecShop.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly int _totalpageItems = 8;
        public StoreController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(int pageIndex)
        {
            IQueryable<Product> query = _context.Products;

            query = query.OrderByDescending(p => p.Id);

            

            if (pageIndex < 1)
            {
                pageIndex = 1; 
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / _totalpageItems);
            query = query.Skip((pageIndex - 1) * _totalpageItems).Take(_totalpageItems);

            var products = query.ToList();
            
            ViewBag.Products = products;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View();
        }
    }
}
