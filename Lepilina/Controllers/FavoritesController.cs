using Microsoft.AspNetCore.Mvc;

namespace Lepilina.Controllers
{
    public class FavoritesController : Controller
    {
        public IActionResult Likes()
        {
            return View();
        }
    }
}