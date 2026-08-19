using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualBasic;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;
using System.Diagnostics.CodeAnalysis;

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
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(Unit unit)
        {
            if(await unitService.UnitSymbolExistsAsync(unit.UnitSymbol))
                {
                ModelState.AddModelError("UnitSymbol", "A unit with this symbol already exists.");
            }
     
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

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var unit = await unitService.GetByIdAsync(id);
            if(unit == null)
                return NotFound();
            return View(unit);
        }
    
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Edit(Guid Id , Unit unit)
        {
            if(Id != unit.Id)
            {
                // If the ID in the URL does not match the ID of the unit being edited, return a 404 Not Found response
                return NotFound();
            }
            if(await unitService.UnitSymbolExistsAsync(unit.UnitSymbol, unit.Id))
            {
                ModelState.AddModelError("UnitSymbol", "This symbol is already taken by another unit.");
            }
            if (ModelState.IsValid)
            {
                try {
                    await unitService.UpdateUnitAsync(unit);
                    TempData["Success"] = "Unit updated successfully!";
                    // redirect back to index page so that the user can see the updated list of units
                    return RedirectToAction(nameof(Index));
                }
                catch(InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the unit: " + ex.Message);

                }
                // Return the view with the unit model to display validation errors or any other issues that occurred during the update process
                return View(unit);
            }
            return View(unit);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]   
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await unitService.DeleteUnitAsync(id);
                if (deleted)
                {
                    TempData["Success"] = "Unit deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Unit not found or could not be deleted.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = "An error occurred while deleting the unit: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
