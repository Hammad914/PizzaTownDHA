using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

     

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(Unit unit)
        {
            ModelState.Remove("IsBaseUnit");

            if (await unitService.UnitSymbolExistsAsync(unit.UnitSymbol))
                ModelState.AddModelError("UnitSymbol", "This symbol is already taken.");

            if (await unitService.UnitNameExistsAsync(unit.UnitName))
                ModelState.AddModelError("UnitName", "This name is already taken.");

            if (ModelState.IsValid)
            {
                try
                {
                    if (await unitService.DisplayOrderExistsAsync(unit.DisplayOrder))
                    {
                        throw new InvalidOperationException($"Display Order '{unit.DisplayOrder}' is already taken. Please choose a different number.");
                    }

                    await unitService.RegisterUnitAsync(unit);
                    TempData["Success"] = "Unit created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "A record with that name or symbol already exists.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(unit);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var unit = await unitService.GetByIdAsync(id);
            if (unit == null)
                return NotFound();
            return View(unit);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Edit(Guid Id, Unit unit)
        {
            ModelState.Remove("IsBaseUnit");

            if (await unitService.UnitSymbolExistsAsync(unit.UnitSymbol, unit.Id))
                ModelState.AddModelError("UnitSymbol", "This symbol is already taken by another unit.");

            if (await unitService.UnitNameExistsAsync(unit.UnitName, unit.Id))
                ModelState.AddModelError("UnitName", "This name is already taken by another unit.");

            if (ModelState.IsValid)
            {
                try
                {
                    if (await unitService.DisplayOrderExistsAsync(unit.DisplayOrder, unit.Id))
                    {
                        throw new InvalidOperationException($"Display Order '{unit.DisplayOrder}' is already taken. Please choose a different number.");
                    }

                    await unitService.UpdateUnitAsync(unit);
                    TempData["Success"] = "Unit updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "A record with that name or symbol already exists.";
                    return RedirectToAction(nameof(Index));
                }
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
            catch (InvalidOperationException)
            {
                TempData["Error"] = "An error occurred while deleting.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}