using Microsoft.AspNetCore.Mvc;

namespace MyBudg.Controllers;

public class HomeController : Controller
{
    public IActionResult Error() => View();
}
