using Microsoft.AspNetCore.Mvc;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;
using PizzaTownDHA.Services;
using PizzaTownDHA.Utilities;

namespace PizzaTownDHA.Controllers
{
    public class StockInsController : Controller
    {
        private readonly IStockInService stockInService;
        private readonly IIngredientService ingredientService;
        private readonly IKitchenLogService kitchenLogService;
        private readonly IProductService productService;
        private readonly IStockAuditService stockAuditService;
        private readonly IUnitService unitService;
        private readonly IUnitConvertorService unitConvertorService;

        public StockInsController(
            IStockInService _stockInService,
            IIngredientService _ingredientService,
            IKitchenLogService _kitchenLogService,
            IProductService _productService,
            IStockAuditService _stockAuditService,
            IUnitService _unitService, 
            IUnitConvertorService _unitConvertorService)
        {
            stockInService = _stockInService;
            ingredientService = _ingredientService;
            kitchenLogService = _kitchenLogService;
            productService = _productService;
            stockAuditService = _stockAuditService;
            unitService = _unitService;
            unitConvertorService = _unitConvertorService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date)
        {
            var businessDate = date ?? DateTime.Now.Date;
            var stockIns = await stockInService.GetByDateAsync(businessDate);
            ViewBag.BusinessDate = businessDate;
            return View(stockIns);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var ingredients = await ingredientService.GetAllAsync();
            var units = await unitService.GetAllAsync();
            ViewData["IngredientList"] = ingredients;
            ViewData["UnitList"] = units;
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(List<Guid> ingredientIds, List<decimal> quantities, DateTime receivedDate, List<Guid>selectedUnitIds)
        {

            if (receivedDate.Date > DateTime.Now.Date)
            {
                TempData["Error"] = "You cannot add stock for a future date.";
                return RedirectToAction(nameof(Create));
            }

            if (ingredientIds == null || ingredientIds.Count == 0)
            {
                TempData["Error"] = "Please select at least one ingredient.";
                return RedirectToAction(nameof(Create));
            }

            if(selectedUnitIds == null || selectedUnitIds.Count == 0)
            {
                TempData["Error"] = "Please Select atleast 1 unit";
                return RedirectToAction(nameof(Create));
            }            
            if(quantities == null || quantities.Count == 0)
            {
                TempData["Error"] = "Please add atleast quantity for 1 unit";
                return RedirectToAction(nameof(Create));
            }
            for (int i = 0; i < quantities.Count; i++)
            {
                if (quantities[i] <= 0)
                {
                    TempData["Error"] = "Quantity cannot be zero or negative.";
                    return RedirectToAction(nameof(Create));
                }
            }
            if (selectedUnitIds.Count != quantities.Count || quantities.Count != ingredientIds.Count)
            {
                TempData["Error"] = "Stock In failed. Please select units for all quantities you entered!";
                return RedirectToAction(nameof(Create));
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
                            }, selectedUnitIds[i]);
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

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var stockIn = await stockInService.GetByIdAsync(id);
            if (stockIn == null)
                return NotFound();
  

            var ingredients = await ingredientService.GetAllAsync();
            var units = await unitService.GetAllAsync();

            ViewData["CurrentUnitId"] = stockIn.Ingredient?.UnitId;
            ViewData["IngredientList"] = ingredients;
            ViewData["UnitList"] = units;
            return View(stockIn);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid Id, StockIn stockIn, Guid selectedUnitId)
        {
            if (Id != stockIn.Id)
            {
                TempData["Error"] = "Invalid stock in record.";
                return RedirectToAction(nameof(Index));
            }

            if (stockIn.QuantityReceived <= 0)
            {
                TempData["Error"] = "Quantity must be greater than 0.";
                return RedirectToAction(nameof(Index));
            }

            if (selectedUnitId == Guid.Empty)
            {
                TempData["Error"] = "Please select a valid unit.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var existing = await stockInService.GetByIdAsync(Id);
                if (existing == null)
                {
                    TempData["Error"] = "Stock In record not found.";
                    return RedirectToAction(nameof(Index));
                }

                // 1. Get the unit and calculate the NEW converted quantity (e.g., 50g -> 0.05 kg)
                var unit = await unitService.GetByIdAsync(selectedUnitId);
                if (unit == null)
                {
                    TempData["Error"] = "Selected unit not found.";
                    return RedirectToAction(nameof(Index));
                }

                var newConvertedQuantity = unitConvertorService.GetConvertedQuantity(unit.ConversionFactor, stockIn.QuantityReceived);

                // 2. Calculate the DELTA (Old - New). 
                var delta = existing.QuantityReceived - newConvertedQuantity;

                // 3. Only block if we are REDUCING stock (delta > 0)
                if (delta > 0)
                {
                    var businessDate = DateTime.Now.Date;
                    var allProducts = await productService.GetAllAsync();
                    var todayLogs = await kitchenLogService.GetAllAsync();
                    var todaysLogs = todayLogs.Where(l => l.DateLogged.Date == businessDate.Date).ToList();

                    decimal usedToday = 0;
                    foreach (var log in todaysLogs)
                    {
                        var product = allProducts.FirstOrDefault(p => p.Id == log.ProductId);
                        if (product != null)
                        {
                            var recipe = product.ProductIngredients.FirstOrDefault(pi => pi.IngredientId == existing.IngredientId);
                            if (recipe != null)
                            {
                                usedToday += recipe.QuantityRequired * log.QuantityMade;
                            }
                        }
                    }

                    // THE SAFETY CHECK: New total stock must be greater than or equal to usedToday
                    var currentTotalStock = await stockInService.GetTotalStockByIngredientIdAsync(existing.IngredientId);
                    var projectedStock = currentTotalStock - delta;

                    if (projectedStock < usedToday)
                    {
                        TempData["Error"] = $"Cannot decrease stock. {usedToday} units already used today. Minimum stock must be {usedToday}.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // 4. SAFE TO UPDATE NOW (Pass the selectedUnitId so it converts correctly)
                await stockInService.UpdateStockInAsync(stockIn, selectedUnitId);

                TempData["Success"] = "Stock In updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
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
                    TempData["Success"] = "Stock In deleted successfully!";
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