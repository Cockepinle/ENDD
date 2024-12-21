using Microsoft.AspNetCore.Mvc;

namespace Lepilina.Controllers
{
    public class SomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
