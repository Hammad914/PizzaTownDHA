using Microsoft.AspNetCore.Mvc;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Controllers
{
    public class StockInsController : Controller
    {
        private readonly IStockInService stockInService;
        private readonly IIngredientService ingredientService;

        public StockInsController(IStockInService _stockInService, IIngredientService _ingredientService)
        {
            stockInService = _stockInService;
            ingredientService = _ingredientService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date)
        {
            var businessDate = date ?? DateTime.Now.Date; // Default to Today

            // ✅ Prevent viewing future dates
            if (businessDate > DateTime.Now.Date)
            {
                TempData["Error"] = "You cannot view future stock records.";
                return RedirectToAction(nameof(Index));
            }

            var stockIns = await stockInService.GetByDateAsync(businessDate);
            ViewBag.BusinessDate = businessDate;
            return View(stockIns);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var ingredients = await ingredientService.GetAllAsync();
            ViewData["IngredientList"] = ingredients;
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(List<Guid> ingredientIds, List<decimal> quantities, DateTime receivedDate)
        {
            // ✅ VALIDATION 1: Future date
            if (receivedDate > DateTime.Now.Date)
            {
                TempData["Error"] = "You cannot add stock for a future date.";
                return RedirectToAction(nameof(Create));
            }

            // ✅ VALIDATION 2: Select at least one ingredient
            if (ingredientIds == null || ingredientIds.Count == 0)
            {
                TempData["Error"] = "Please select at least one ingredient.";
                return RedirectToAction(nameof(Create));
            }

            // ✅ VALIDATION 3: Negative quantities
            for (int i = 0; i < quantities.Count; i++)
            {
                if (quantities[i] <= 0)
                {
                    TempData["Error"] = "Quantity cannot be zero or negative. Please enter a valid amount.";
                    return RedirectToAction(nameof(Create));
                }
            }

            try
            {
                for (int i = 0; i < ingredientIds.Count; i++)
                {
                    if (quantities[i] > 0)
                    {
                        await stockInService.AddStockInAsync(new StockIn
                        {
                            Id = Guid.NewGuid(),
                            IngredientId = ingredientIds[i],
                            QuantityReceived = quantities[i],
                            ReceivedDate = receivedDate,
                            CreatedBy = "System"
                        });
                    }
                }

                TempData["Success"] = "Stock In added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await stockInService.DeleteStockInAsync(id);
                if (deleted)
                {
                    TempData["Success"] = "Stock In record deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Stock In record not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}