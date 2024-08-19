﻿using System.Text.Json;

namespace ElecShop.Services
{
    public class CartHelper
    {
        public static Dictionary<int, int> GetCartDictionary(HttpRequest request, HttpResponse response)
        {

            string cookieValue = request.Cookies["shopping_cart"] ?? "";

            try
            {
                var cart = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cookieValue));
                Console.WriteLine($"[CartHelper] cart={cookieValue} -> {cart}");
                var dictionary = JsonSerializer.Deserialize<Dictionary<int, int>>(cart);
                if (dictionary is not null)
                {
                    return dictionary;
                }
            }
            catch (Exception)
            {
                if (cookieValue.Length < 0)
                {
                    response.Cookies.Delete("shopping_cart");
                }
                throw;
            }

            return new Dictionary<int, int>();
        }

        public static int GetCartSize(HttpRequest request, HttpResponse response)
        {
            int cartSize = 0;

            var cartDictionary = GetCartDictionary(request, response);
            foreach (var item in cartDictionary)
            {
                cartSize += item.Value;
            }

            return cartSize;
        }
    }
}
