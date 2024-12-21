using Lepilina.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Sitecore.FakeDb;
using System;
using System.Security.Claims;

namespace Lepilina.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var totalSales = _context.Products.Sum(p => p.stocks_quantity);
            var categorySales = _context.Products
                .GroupBy(p => p.category_id)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Count = g.Sum(p => p.stocks_quantity) 
                })
                .ToList();
            ViewBag.TotalSales = totalSales;
            ViewBag.CategorySales = JsonConvert.SerializeObject(categorySales);
            var theme = HttpContext.Items["Theme"]?.ToString();
            ViewBag.Theme = theme ?? "light";

            var userName = User.Identity.Name;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.IsInRole("Администратор") ? "Администратор" :
                           User.IsInRole("Менеджер") ? "Менеджер" :
                           "Покупатель";

            ViewBag.Greeting = $"Здравствуйте, {userName}!";
            ViewBag.UserId = userId;

            var products = await _context.Products
                                          .Include(p => p.Images)
                                          .Include(p => p.Category)
                                          .ToListAsync();

            var userPositionId = await _context.Position
                                                .Where(p => p.position == userRole)
                                                .Select(p => p.position_id)
                                                .FirstOrDefaultAsync();

            ViewBag.UserPositionId = userPositionId;

            var users = await _context.Users.Include(u => u.Position).ToListAsync();

            var account = await _context.Accounts.Include(u => u.Users).ToListAsync();
            var category = await _context.Category.Include(u => u.Products).ToListAsync();


            var model = (products.AsEnumerable(), users.AsEnumerable(), account.AsEnumerable(), category.AsEnumerable());

            return View(model);
        }

        [HttpPost]
        public IActionResult ToggleTheme()
        {
            var currentTheme = HttpContext.Items["Theme"]?.ToString() ?? "light";
            var newTheme = currentTheme == "dark" ? "light" : "dark";
            Response.Cookies.Append("Theme", newTheme);

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Catalog()
        {
            var tem = HttpContext.Items["Theme"]?.ToString();
            ViewBag.Theme = tem;

            var products = await _context.Products.Include(p => p.Category).ToListAsync();

            var productViewModels = new List<ProductViewModel>();

            foreach (var product in products)
            {
                var images = await _context.Images.Where(i => i.products_id == product.products_id).ToListAsync();
                productViewModels.Add(new ProductViewModel
                {
                    Product = product,
                    Images = images
                });
            }

            return View(productViewModels);
        }
        public IActionResult GetImage(int id)
        {
            var image = _context.Images.Find(id);

            if (image != null && !string.IsNullOrEmpty(image.image_data))
            {
                return File(image.image_data, "image/jpeg");
            }

            return NotFound();
        }
        public IActionResult SignIn()
        {
            if (HttpContext.Session.Keys.Contains("AuthUser"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                Accounts accounts = await _context.Accounts.FirstOrDefaultAsync(u => u.logins == model.Login && u.passwords == model.Password);
                if (accounts != null) 
                {
                    HttpContext.Session.SetString("AuthUser", model.Login);
                    await Authenticate(model.Login);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Некорректный логин и(или) пароль");
                }
            }
            return View(model);
        }

        public async Task Authenticate(string userName)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.logins == userName);

            if (account == null)
            {
                throw new Exception("Пользователь не найден.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.users_id == account.users_id);

            if (user == null)
            {
                throw new Exception("Пользователь не найден в таблице Users.");
            }

            var roles = await _context.Position
                                       .Where(ur => ur.position_id == user.position_id) 
                                       .ToListAsync();

            if (roles == null || !roles.Any())
            {
                throw new Exception("Роли не найдены для данного пользователя.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role.position));
                Console.WriteLine($"Adding role: {role.position}");
            }

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie");

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("AuthUser");
            return RedirectToAction("SignIn");
        }
        [HttpGet("signup")]

        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost("signup")]

        public async Task<IActionResult> SignUp(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var existingAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.logins == model.Login);
                if (existingAccount != null)
                {
                    ModelState.AddModelError("", "Пользователь с таким логином уже существует."); 
                    return View(model);
                }

                var account = new Accounts
                {
                    logins = model.Login,
                    passwords = model.Password
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync(); 

                var user = new Users
                {
                    sername = model.Sername,
                    names = model.Names,
                    patronymic = model.Patronymic,
                    position_id = 3 
                };

                _context.Users.Add(user);

                try
                {
                    await _context.SaveChangesAsync(); 

                    account.users_id = user.users_id;
                    await _context.SaveChangesAsync(); 
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ошибка при создании пользователя: " + ex.Message);
                    return View(model);
                }

                return RedirectToAction("SignIn");
            }

            return View(model);
        }

    }
}