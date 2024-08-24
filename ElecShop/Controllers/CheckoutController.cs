using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json.Nodes;

namespace ElecShop.Controllers
{
    public class CheckoutController : Controller
    {
        private string PaypalClientId { get; set; } = "";
        private string PaypalSecret { get; set; } = "";
        private string PaypalUrl { get; set; } = "";

        public CheckoutController(IConfiguration configuration)
        {
            PaypalClientId = configuration["PayPalSettings:ClientId"]!;
            PaypalSecret = configuration["PayPalSettings:Secret"]!;
            PaypalUrl = configuration["PayPalSettings:Url"]!;
        }

        public IActionResult Index()
        {

            ViewBag.PaypalClientId = PaypalClientId;
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> CreateOrder([FromBody] JsonObject data)
        {
            var totalAmount = data?["amount"]?.ToString();
            if (totalAmount is null)
            {
                return new JsonResult(new { Id = "" });
            }


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
            if (orderId is null)
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
                            return new JsonResult("success");
                        }
                    }
                }
            }

                return new JsonResult("error");
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
