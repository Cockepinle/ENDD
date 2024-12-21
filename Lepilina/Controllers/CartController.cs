using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using Lepilina.Models;
using Microsoft.EntityFrameworkCore;

namespace Lepilina.Controllers
{
    public class CartController : Controller
    {
        private const string CartCookieName = "Cart";
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var product = _context.Products
                .Include(p => p.Images) // З
                .FirstOrDefault(p => p.products_id == productId);

            if (product == null) return NotFound();

            var cart = GetCart();
            var cartItem = cart.Find(item => item.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.products_id,
                    Quantity = quantity,
                    ProductName = product.name_products,
                    ProductDescription = product.descriptions,
                    ProductPrice = product.price,
                    ProductImage = product.Images.FirstOrDefault()?.image_data 
                });
            }

            SaveCart(cart);
            return Ok(new { success = true }); 
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var cartItem = cart.Find(item => item.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                SaveCart(cart); 
            }

            return RedirectToAction("ViewCart");
        }

        private List<CartItem> GetCart()
        {
            var cartJson = Request.Cookies[CartCookieName];
            var cart = string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

            return cart;
        }

        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            Response.Cookies.Append(CartCookieName, cartJson, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30) });
        }

        public IActionResult ViewCart()
        {
            var cart = GetCart();
            var cartItems = new List<CartItem>();

            foreach (var item in cart)
            {
                var product = _context.Products.Include(p => p.Images).FirstOrDefault(p => p.products_id == item.ProductId);

                if (product != null)
                {
                    cartItems.Add(new CartItem
                    {
                        ProductId = product.products_id,
                        Quantity = item.Quantity,
                        ProductName = product.name_products,
                        ProductDescription = product.descriptions,
                        ProductPrice = product.price,
                        ProductImage = product.Images.FirstOrDefault()?.image_data
                    });
                }
            }

            return View(cartItems); 
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var cartItem = cart.Find(item => item.ProductId == productId);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCart(cart);
            }

            return RedirectToAction("ViewCart");
        }

        [HttpPost]
        public IActionResult CreateOrder()
        {
            var cart = GetCart();

            if (cart.Count == 0)
                return RedirectToAction("ViewCart");

            Response.Cookies.Delete(CartCookieName); 

            return RedirectToAction("OrderSuccess");
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Checkout(OrderViewModel model)
        {
            var cart = GetCart(); 

            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("ViewCart");
            }

                _context.SaveChanges();

                Response.Cookies.Delete(CartCookieName); 

            
            var orderViewModel = new OrderViewModel
            {
                CartItems = cart, 
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                ShippingAddress = model.ShippingAddress
            };

            return View(orderViewModel);
        }

        [HttpGet]
        public JsonResult GetSalesData()
        {
            var totalSalesCount = _context.Products.Count();

            var categorySalesData = _context.Products
                .Include(p => p.Category) 
                .GroupBy(p => p.category_id) 
                .Select(g => new
                {
                    CategoryName = g.FirstOrDefault().Category.name_category, 
                    Count = g.Count() 
                })
                .ToList();

            return Json(new { totalSalesCount, categorySalesData });
        }
        [HttpGet]
        public IActionResult ExportSalesReport()
        {
            var totalSalesCount = _context.Products.Count();

            var categorySalesData = _context.Products
            .Include(p => p.Category)
            .GroupBy(p => p.category_id)
            .Select(g => new CategorySalesData
            {
                CategoryName = g.FirstOrDefault().Category.name_category,
                Count = g.Count()
            })
            .ToList();
            foreach (var category in categorySalesData)
            {
                Console.WriteLine($"Category: {category.CategoryName}, Count: {category.Count}");
            }

            ViewBag.TotalSales = totalSalesCount;
            ViewBag.CategorySales = categorySalesData;
            var pdfService = new PdfExportService();
            var pdfStream = pdfService.CreateSalesReport(totalSalesCount, categorySalesData);

            return File(pdfStream.ToArray(), "application/pdf", "ОтчетПоПродажам.pdf");
        }

    }
}
