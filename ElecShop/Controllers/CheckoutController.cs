using ElecShop.Models;
using ElecShop.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json.Nodes;

namespace ElecShop.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private string PaypalClientId { get; set; } = "";
        private string PaypalSecret { get; set; } = "";
        private string PaypalUrl { get; set; } = "";

        private readonly decimal _shippingFee;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(IConfiguration configuration, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            PaypalClientId = configuration["PayPalSettings:ClientId"]!;
            PaypalSecret = configuration["PayPalSettings:Secret"]!;
            PaypalUrl = configuration["PayPalSettings:Url"]!;

            _shippingFee = configuration.GetValue<decimal>("CartSettings:ShippingFee");

            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {

            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, _context);
            decimal total = CartHelper.GetSubtotal(cartItems) + _shippingFee;


            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            TempData.Keep();


            ViewBag.DeliveryAddress = deliveryAddress;
            ViewBag.Total = total;

            ViewBag.PaypalClientId = PaypalClientId;
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> CreateOrder()
        {
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, _context);
            decimal totalAmount = CartHelper.GetSubtotal(cartItems) + _shippingFee;


            JsonObject createOrderRequest = new JsonObject();
            createOrderRequest.Add("intent", "CAPTURE");

            JsonObject amount = new JsonObject();
            amount.Add("currency_code", "USD");
            amount.Add("value", totalAmount);

            JsonObject purchaseUnit1 = new JsonObject();
            purchaseUnit1.Add("amount", amount);

            JsonArray purchaseUnits = new JsonArray();
            purchaseUnits.Add(purchaseUnit1);

            createOrderRequest.Add("purchase_units", purchaseUnits);


            string accessToken = await GetPaypalAccessToken();

            string url = $"{PaypalUrl}/v2/checkout/orders";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");


                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent(createOrderRequest.ToString(), null, "application/json");


                var httpResponse = await client.SendAsync(requestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);

                    if (jsonResponse is not null)
                    {
                        string paypalOrderId = jsonResponse["id"]?.ToString() ?? "";

                        return new JsonResult(new { Id = paypalOrderId });
                    }
                }
            }


            return new JsonResult(new { Id = "" });
        }

        [HttpPost]
        public async Task<JsonResult> CompleteOrder([FromBody] JsonObject data)
        {
            var orderId = data?["orderID"]?.ToString();
            var deliveryAddress = data?["deliveryAddress"]?.ToString();

            if (orderId is null || deliveryAddress is null)
            {
                return new JsonResult("error");
            }

            string accessToken = await GetPaypalAccessToken();

            string url = $"{PaypalUrl}/v2/checkout/orders/{orderId}/capture";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("", null, "application/json");

                var httpResponse = await client.SendAsync(requestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);

                    if (jsonResponse is not null)
                    {
                        string paypalOrderStatus = jsonResponse["status"]?.ToString() ?? "";
                        if (paypalOrderStatus == "COMPLETED")
                        {
                            await SaveOrderAsync(jsonResponse.ToString(), deliveryAddress);
                            return new JsonResult("success");

                        }
                    }
                }
            }

                return new JsonResult("error");
        }

        private async Task SaveOrderAsync(string paypalResponse, string deliveryAddress)
        {
            var cartItems = CartHelper.GetCartItems(Request, Response, _context);

            var appUser = await _userManager.GetUserAsync(User);
            if (appUser is null)
            {
                return;
            }

            var order = new Order
            {
                ClientId = appUser.Id,
                Items = cartItems,
                ShippingFee = _shippingFee,
                DeliveryAddress = deliveryAddress,
                PaymentMethod = "paypal",
                PaymentStatus = "accepted",
                PaymentDetails = paypalResponse,
                OrderStatus = "pending",
                CreatedAt = DateTime.Now
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            Response.Cookies.Delete("shopping_cart");

        }




        //public async Task<string> Token()
        //{
        //    return await GetPaypalAccessToken();
        //}

        private async Task<string> GetPaypalAccessToken()
        {
            string accessToken = "";

            string url = $"{PaypalUrl}/v1/oauth2/token";

            using (var client = new HttpClient())
            {
                string credentials64 =
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{PaypalClientId}:{PaypalSecret}"));

                client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials64}");

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("grant_type=client_credentials", null, "application/x-www-form-urlencoded");
                var httpResponse = await client.SendAsync(requestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();

                    var jsonResponse = JsonNode.Parse(strResponse);
                    if (jsonResponse is not null)
                    {
                        accessToken = jsonResponse["access_token"]?.ToString() ?? "";
                    }
                }

            }


            return accessToken;
        }

    }
}
