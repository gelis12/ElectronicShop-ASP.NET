﻿using ElecShop.Models;
using ElecShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElecShop.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly int pageSize = 5;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(int pageIndex, string? search, string? column, string? orderBy)
        {
            IQueryable<Product> querry = _context.Products;

            if (search is not null)
            {
                querry = querry.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));
            }

            string[] validColumns = { "Id", "Name", "Brand", "Category", "Price", "CreatedAt" };
            string[] validOrderBy = { "desc", "asc" };

            if (!validColumns.Contains(column))
            {
                column = "Id";
            }

            if (!validOrderBy.Contains(orderBy))
            {
                orderBy = "desc";
            }



            switch (column)
            {
                case "Name":
                    if (orderBy == "asc")
                    {
                        querry = querry.OrderBy(p => p.Name);
                    }
                    else
                    {
                        querry = querry.OrderByDescending(p => p.Name);
                    }
                    break;
                case "Brand":
                    if (orderBy == "asc")
                    {
                        querry = querry.OrderBy(p => p.Brand);
                    }
                    else
                    {
                        querry = querry.OrderByDescending(p => p.Brand);
                    }
                    break;
                case "Category":
                    if (orderBy == "asc")
                    {
                        querry = querry.OrderBy(p => p.Category);
                    }
                    else
                    {
                        querry = querry.OrderByDescending(p => p.Category);
                    }
                    break;
                case "Price":
                    if (orderBy == "asc")
                    {
                        querry = querry.OrderBy(p => p.Price);
                    }
                    else
                    {
                        querry = querry.OrderByDescending(p => p.Price);
                    }
                    break;
                case "CreatedAt":
                    if (orderBy == "asc")
                    {
                        querry = querry.OrderBy(p => p.CreatedAt);
                    }
                    else
                    {
                        querry = querry.OrderByDescending(p => p.CreatedAt);
                    }
                    break;
                default:
                    if (orderBy == "asc")
                    {
                        querry = querry.OrderBy(p => p.Id);
                    }
                    else
                    {
                        querry = querry.OrderByDescending(p => p.Id);
                    }
                    break;
            }
            

            //querry = querry.OrderByDescending(p => p.Id);



            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = querry.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);
            querry = querry.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var products = querry.ToList();

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages;

            ViewData["Search"] = search ?? "";

            ViewData["Column"] = column;
            ViewData["OrderBy"] = orderBy;

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
                ModelState.AddModelError("ImageFile", "The image is necessary.");
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

        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);

            if (product is null)
            {
                return RedirectToAction("Index", "Products");
            }

            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("dd/MM/yyyy");

            return View(productDto);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto) 
        {
            var product = _context.Products.Find(id);

            if (product is null)
            {
                return RedirectToAction("Index", "Products");
            }

            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("dd/MM/yyyy");

                return View(productDto);
            }

            string newFileName = product.ImageFileName;
            if (productDto.ImageFile is not null)
            {
                newFileName = DateTime.Now.ToString("ddMMyyyyHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFile.FileName);

                string imageFullPath = _webHostEnvironment.WebRootPath + "/products/" + newFileName;

                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                string oldImageFullPath = _webHostEnvironment.WebRootPath + "/products/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }

            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            _context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }

        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);

            if (product is null)
            {
                return RedirectToAction("Index", "Products");
            }

            string imageFullPath = _webHostEnvironment.WebRootPath + "/products/" + product.ImageFileName;
            System.IO.File.Delete(imageFullPath);

            _context.Products.Remove(product);
            _context.SaveChanges(true);

            return RedirectToAction("Index", "Products");
        }



    }
}
