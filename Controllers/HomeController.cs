using Microsoft.AspNetCore.Mvc;

namespace PizzaTownDHA.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}