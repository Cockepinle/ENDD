using Microsoft.AspNetCore.Mvc;

namespace Lepilina.Controllers
{
    public class Account : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
