using Lepilina.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lepilina.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var theme = HttpContext.Items["Theme"]?.ToString();
            ViewBag.Theme = theme ?? "light";

            var userName = User.Identity.Name;
            var userRole = User.IsInRole("Администратор") ? "Администратор" :
                           User.IsInRole("Менеджер") ? "Менеджер" :
                           "Покупатель";

            ViewBag.Greeting = $"Здравствуйте, {userName}!";


            var products = await _context.Products
                                          .Include(p => p.Images)
                                          .Include(p => p.Category)
                                          .ToListAsync();



            var userPositionId = await _context.Position
                                                .Where(p => p.position == userRole)
                                                .Select(p => p.position_id)
                                                .FirstOrDefaultAsync();

            ViewBag.UserPositionId = userPositionId;
            var category = await _context.Category.ToListAsync();
            var users = await _context.Users.Include(u => u.Position).ToListAsync();
            var viewModel = new DeleteViewModel
            {
                Users = users,
                Categories = category
            };
            var model = (products.AsEnumerable(), users.AsEnumerable(), category.AsEnumerable(), viewModel);

            return View(model);
        }

        [Authorize(Roles = "Менеджер,Администратор")]
        public IActionResult Create()
        {
            var product = new Products();
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Менеджер,Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Products product)
        {
                _context.Add(product);
                await _context.SaveChangesAsync();

                foreach (var image in product.Images)
                {
                    if (!string.IsNullOrEmpty(image.image_data))
                    {
                        image.products_id = product.products_id; 
                        _context.Images.Add(image);
                    }
                }
            return View(product);
        }

        [Authorize(Roles = "Менеджер,Администратор")]
        public IActionResult CreateUs()
        {
            var positions = _context.Position.ToList(); 
            ViewBag.Positions = positions;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Менеджер,Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUs(Users users, string logins, string password)
        {

                _context.Add(users);
                await _context.SaveChangesAsync();

                var acc = new Accounts
                {
                    users_id = users.users_id,
                    logins = logins,
                    passwords = password 
                };

                _context.Accounts.Add(acc); 
                await _context.SaveChangesAsync();           
                ViewBag.Positions = _context.Position.ToList();
                return View(users);
        }

        [Authorize(Roles = "Менеджер,Администратор")]
        public IActionResult CreateCa()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Менеджер,Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCa(Category category)
        {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return View(category); 
        }


        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.Include(p => p.Images) 
        .FirstOrDefaultAsync(p => p.products_id == id);

            if (product == null) 
            {
                return NotFound();
            }

            return View(product);
        }
        [HttpPost]
        [Authorize(Roles = "Менеджер,Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Products product)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }

            var existingProduct = await _context.Products.Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.products_id == id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.name_products = product.name_products;
            existingProduct.price = product.price;
            existingProduct.descriptions = product.descriptions;

            foreach (var image in product.Images)
            {
                var existingImage = existingProduct.Images.FirstOrDefault(i => i.images_id == image.images_id);
                if (existingImage != null)
                {
                    existingImage.image_data = image.image_data; 
                }
                else
                {
                    existingProduct.Images.Add(image);
                }
            }

            var imagesToRemove = existingProduct.Images.Where(i => !product.Images.Any(pi => pi.images_id == i.images_id)).ToList();
            foreach (var image in imagesToRemove)
            {
                _context.Entry(image).State = EntityState.Deleted; 
            }

            await _context.SaveChangesAsync();

            return View(product); 
        }

        public async Task<IActionResult> EditFirst(int id)
        {
            var product = await _context.Users.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Менеджер,Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFirst(int id, Users users)
        {
            if (id != users.users_id)
            {
                return NotFound();
            }

            _context.Update(users);
            await _context.SaveChangesAsync();

            return View(users);
        }
        public async Task<IActionResult> EditAcc(int id)
        {
            var product = await _context.Accounts.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Менеджер,Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAcc(int id, Accounts accounts)
        {
            if (id != accounts.users_id)
            {
                return NotFound();
            }

            _context.Update(accounts);
            await _context.SaveChangesAsync();

            return View(accounts);
        }

        public async Task<IActionResult> EditCa(int id)
        {
            var product = await _context.Category.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Менеджер,Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCa(int id, Category category)
        {
            if (id != category.category_id)
            {
                return NotFound();
            }

            _context.Update(category);
            await _context.SaveChangesAsync();

            return View(category);
        }
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.products_id == id); 
        }



        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                var images = await _context.Images.Where(pi => pi.products_id == id).ToListAsync();
                _context.Images.RemoveRange(images);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return View();
        }

        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> DeleteFirst(int id)
        {
            var user = await _context.Users.Include(u => u.Position).FirstOrDefaultAsync(u => u.users_id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> DeleteConfirmedFirst(int id)
        {
            var user = await _context.Users.Include(u => u.Position).FirstOrDefaultAsync(u => u.users_id == id);

            if (user == null)
            {
                return NotFound(); 
            }

            var accounts = await _context.Accounts.Where(a => a.users_id == id).ToListAsync();
            _context.Accounts.RemoveRange(accounts);

            if (user.Position != null)
            {
                var usersWithSamePosition = await _context.Users.AnyAsync(u => u.position_id == user.Position.position_id && u.users_id != user.users_id);
                if (!usersWithSamePosition) 
                {
                    _context.Position.Remove(user.Position);
                }
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return View();
        }

        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> DeleteCa(int id)
        {
            var category = await _context.Category.Include(c => c.Products).FirstOrDefaultAsync(c => c.category_id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> DeleteConfirmedCa(int id)
        {
            var category = await _context.Category.Include(c => c.Products).FirstOrDefaultAsync(c => c.category_id == id);
            if (category == null)
            {
                return NotFound();
            }

            if (category.Products != null && category.Products.Any())
            {
                _context.Products.RemoveRange(category.Products);
            }

            _context.Category.Remove(category);

            await _context.SaveChangesAsync();

            return View();
        }

    

    }
}