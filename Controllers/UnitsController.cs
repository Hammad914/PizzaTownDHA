using Microsoft.AspNetCore.Mvc;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Services;

namespace PizzaTownDHA.Controllers
{
    public class UnitsController : Controller
    {
        private readonly IUnitService unitService;
        public UnitsController(IUnitService _unitService)
        {
            unitService = _unitService;
        }
        public async Task<IActionResult> Index()
        {
            var units = await unitService.GetAllAsync();
            return View(units);
        }
    }
}
