using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Controllers
{
    public class UnitsController : Controller
    {
        private readonly IUnitService unitService;
        public UnitsController(IUnitService _unitService)
        {
            unitService = _unitService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var units = await unitService.GetAllAsync();
            if (units == null)
                return NotFound();
            return View(units);
        }

        // Not needed for now, but can be implemented later if needed it returns data for a specific unit by its ID and we need details.cshtml view to display the details of a unit.

        //[HttpGet]
        //public async Task<IActionResult> Details(Guid id)
        //{
        //    var response = await unitService.GetByIdAsync(id);
        //    if (response == null)
        //        return NotFound();
        //    return View(response);
        //}


        // This is required to open a model for creating a new unit and to handle the form submission for creating a new unit. It also includes validation and error handling.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Unit unit)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await unitService.RegisterUnitAsync(unit);
                    TempData["Success"] = "Unit created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch(InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(unit);
        }

    }
}
