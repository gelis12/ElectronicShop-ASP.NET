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
        public IActionResult Index(int pageIndex, string? search, string? brand, string? category, string? sort)
        {
            IQueryable<Product> query = _context.Products;

            if (search is not null && search.Length > 0)
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            if (brand is not null && brand.Length > 0)
            {
                query = query.Where(p => p.Brand.Contains(brand));
            }

            if (category is not null && category.Length > 0)
            {
                query = query.Where(p => p.Category.Contains(category));
            }

            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price); break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price); break;
                default:
                    query = query.OrderByDescending(p => p.Id); break;
            }

            //query = query.OrderByDescending(p => p.Id);

            

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

            var storeSearchModel = new StoreSearchModel()
            {
                Search = search,
                Brand = brand,
                Category = category,
                Sort = sort
            };

            return View(storeSearchModel);
        }
    }
}
