﻿using ElecShop.Models;
using ElecShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace ElecShop.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/Orders/{action=Index}/{id?}")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly int _itemsPerPages = 5;

        public AdminOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(int pageIndex)
        {
            IQueryable<Order> query = _context.Orders.Include(o => o.Client).Include(o => o.Items).OrderByDescending(o => o.Id);

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
