using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Lepilina.Models;

namespace Lepilina.Controllers
{
    public class ManagerController : Controller
    {
        private readonly AppDbContext _context;

        public ManagerController(AppDbContext context)
        {
            _context = context;
        }

        // Метод для отображения графиков
        public IActionResult Dashboard()
        {
            // Получаем общее количество проданных товаров (допустим, это сумма всех товаров, которые были куплены)
            var totalSales = _context.Products.Sum(p => p.stocks_quantity); // Здесь предполагается, что stocks_quantity - это количество на складе

            // Получаем распределение продаж по категориям
            var categorySales = _context.Products
                .GroupBy(p => p.category_id)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Count = g.Sum(p => p.stocks_quantity) // Или используйте другую метрику, если необходимо
                })
                .ToList();

            ViewBag.TotalSales = totalSales;
            ViewBag.CategorySales = JsonConvert.SerializeObject(categorySales);

            return View();
        }

        // Метод для оформления заказа
        [HttpPost]
        public IActionResult Checkout(OrderViewModel orderViewModel)
        {
            // Получаем корзину из куков (реализуйте метод GetCart)
            var cart = GetCart();

            // Проверка на наличие товаров в корзине
            if (cart == null || !cart.Any())
            {
                ModelState.AddModelError("", "Корзина пуста. Пожалуйста, добавьте товары перед оформлением заказа.");
                return View(orderViewModel); // Возвращаем обратно на форму
            }

            // Проверка валидности модели
            if (!ModelState.IsValid)
            {
                return View(orderViewModel); // Если модель не валидна, возвращаем обратно на форму
            }

            // Обработка заказа (уменьшаем количество на складе)
            foreach (var item in cart)
            {
                var product = _context.Products.FirstOrDefault(p => p.products_id == item.ProductId);
                if (product != null)
                {
                    // Проверка наличия достаточного количества товара на складе
                    if (product.stocks_quantity >= item.Quantity)
                    {
                        // Обновляем количество на складе
                        product.stocks_quantity -= item.Quantity; // Уменьшаем количество на складе
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Недостаточно товара '{product.name_products}' на складе.");
                        return View(orderViewModel); // Возвращаем обратно на форму с ошибкой
                    }
                }
            }

            // Сохранение изменений в базе данных
            _context.SaveChanges();

            // Очистка корзины после оформления заказа
            Response.Cookies.Delete("Cart");

            return RedirectToAction("OrderSuccess");
        }

        private List<CartItem> GetCart()
        {
            var cartJson = Request.Cookies["Cart"];
            return string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}