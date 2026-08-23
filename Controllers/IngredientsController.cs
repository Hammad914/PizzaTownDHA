using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Controllers
{
    public class IngredientsController : Controller
    {
        private readonly IIngredientService ingredientService;
        private readonly IUnitService unitService;

        public IngredientsController(IIngredientService _ingredientService, IUnitService _unitService)
        {
            ingredientService = _ingredientService;
            unitService = _unitService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var ingredients = await ingredientService.GetAllAsync();
            return View(ingredients);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var units = await unitService.GetAllAsync();
            ViewData["UnitSelectList"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(units, "Id", "UnitSymbol");
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(Ingredient ingredient)
        {
            if (!string.IsNullOrWhiteSpace(ingredient.Name))
            {
                if (await ingredientService.IngredientNameExistsAsync(ingredient.Name))
                {
                    TempData["Error"] = "An ingredient with this name already exists.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ingredient.Tolerance > 75)
            {
                TempData["Error"] = "Tolerance cannot be more than 75%.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await ingredientService.RegisterIngredientAsync(ingredient);
                TempData["Success"] = "Ingredient created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var ingredient = await ingredientService.GetByIdAsync(id);
            if (ingredient == null)
                return NotFound();

            var units = await unitService.GetAllAsync();
            ViewData["UnitSelectList"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(units, "Id", "UnitSymbol", ingredient.UnitId);
            return View(ingredient);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Edit(Guid Id, Ingredient ingredient)
        {
            if (Id != ingredient.Id)
                return NotFound();

            // 1. Check for Duplicate Name (excluding itself)
            if (!string.IsNullOrWhiteSpace(ingredient.Name))
            {
                if (await ingredientService.IngredientNameExistsAsync(ingredient.Name, ingredient.Id))
                {
                    TempData["Error"] = "An ingredient with this name already exists.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ingredient.Tolerance > 75)
            {
                TempData["Error"] = "Tolerance cannot be more than 75%. Please enter a value between 0 and 75.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await ingredientService.UpdateIngredientAsync(ingredient);
                    TempData["Success"] = "Ingredient updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "A record with that name already exists.";
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["Error"] = "Please fill in all required fields correctly.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await ingredientService.DeleteIngredientAsync(id);
                if (deleted)
                {
                    TempData["Success"] = "Ingredient deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Ingredient not found or could not be deleted.";
                }
            }
            catch (InvalidOperationException)
            {
                TempData["Error"] = "An error occurred while deleting the ingredient.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}