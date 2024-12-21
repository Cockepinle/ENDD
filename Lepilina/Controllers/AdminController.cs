using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Администратор")]
public class AdminsController : Controller
{
    public IActionResult Dashboard()
    {
        return View();
    }
}