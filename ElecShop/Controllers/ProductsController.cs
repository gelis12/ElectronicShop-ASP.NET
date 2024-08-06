using ElecShop.Models;
using ElecShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElecShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var products = _context.Products.OrderByDescending(p=>p.Id).ToList();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Imaginea este necesara");
            }

            if (!ModelState.IsValid)
            {
                return View(productDto);
            }

            string newFileName = DateTime.Now.ToString("ddMMyyyymmssfff");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

            string imageFullPath = _webHostEnvironment.WebRootPath + "/products/" + newFileName;

            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now,
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            return RedirectToAction("Index","Products");
        }

    }
}
