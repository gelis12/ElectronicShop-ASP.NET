using ElecShop.Models;
using ElecShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ElecShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly decimal _shippingFee;

        public CartController(ApplicationDbContext context, IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _shippingFee = configuration.GetValue<decimal>("CartSettings:ShippingFee");
        }
        public IActionResult Index()
        {
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, _context);
            decimal subtotal = CartHelper.GetSubtotal(cartItems);

            ViewBag.CartItems = cartItems;
            ViewBag.ShippingFee = _shippingFee;
            ViewBag.Subtotal = subtotal;
            ViewBag.Total = subtotal + _shippingFee;


            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult Index(CheckoutDto model)
        {
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, _context);
            decimal subtotal = CartHelper.GetSubtotal(cartItems);

            ViewBag.CartItems = cartItems;
            ViewBag.ShippingFee = _shippingFee;
            ViewBag.Subtotal = subtotal;
            ViewBag.Total = subtotal + _shippingFee;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (cartItems.Count == 0)
            {
                ViewBag.ErrorMessage = "Your cart is empty";
                return View(model);
            }

            TempData["DeliveryAddress"] = model.DeliveryAddress;
            TempData["PaymentMethod"] = model.PaymentMethod;


            return RedirectToAction("Confirm");

        }

        public IActionResult Confirm()
        {
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, _context);
            decimal total = CartHelper.GetSubtotal(cartItems) + _shippingFee;
            int carsize = 0;
            foreach (var item in cartItems)
            {
                carsize += item.Quantity;
            }

            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            string paymentMethod = TempData["PaymentMethod"] as string ?? "";
            TempData.Keep();

            if (carsize == 0 || deliveryAddress.Length == 0 || paymentMethod.Length == 0)
            {
                return RedirectToAction("Index", "Home");   
            }

            ViewBag.DeliveryAddress = deliveryAddress;
            ViewBag.PaymentMethod = paymentMethod;
            ViewBag.Total = total;
            ViewBag.CartSize = carsize;


            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Confirm(int any)
        {
            var cartItems = CartHelper.GetCartItems(Request, Response, _context);

            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            string paymentMethod = TempData["PaymentMethod"] as string ?? "";
            TempData.Keep();

            if (cartItems.Count == 0 || deliveryAddress.Length == 0 || paymentMethod.Length == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var appUser = await _userManager.GetUserAsync(User);
            if (appUser is null)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = new Order
            {
                ClientId = appUser.Id,
                Items = cartItems,
                ShippingFee = _shippingFee,
                DeliveryAddress = deliveryAddress,
                PaymentMethod = paymentMethod,
                PaymentStatus = "pending",
                PaymentDetails = "",
                OrderStatus = "created",
                CreatedAt = DateTime.Now,
            };

            _context.Orders.Add(order);
            _context.SaveChanges();


            Response.Cookies.Delete("shopping_cart");

            ViewBag.SuccessMessage = "Order created successfully";

            return View();
        }
    }
}
