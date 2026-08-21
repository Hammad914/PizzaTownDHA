using Microsoft.AspNetCore.Mvc;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Controllers
{
    public class StockReconciliationController : Controller
    {
        private readonly IIngredientService ingredientService;
        private readonly IStockAuditService stockAuditService;
        private readonly IProductService productService;
        private readonly IKitchenLogService kitchenLogService;

        public StockReconciliationController(
            IIngredientService _ingredientService,
            IStockAuditService _stockAuditService,
            IProductService _productService,
            IKitchenLogService _kitchenLogService)
        {
            ingredientService = _ingredientService;
            stockAuditService = _stockAuditService;
            productService = _productService;
            kitchenLogService = _kitchenLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date)
        {
            var businessDate = date ?? GetBusinessDate(DateTime.Now);

            // Prevent viewing future dates
            if (businessDate > GetBusinessDate(DateTime.Now))
            {
                TempData["Error"] = "You cannot view or edit future stock counts.";
                return RedirectToAction(nameof(Index));
            }

            var ingredients = await ingredientService.GetAllAsync();
            var todayAudits = await stockAuditService.GetByDateAsync(businessDate);

            var model = new List<StockReconciliationRow>();

            foreach (var ingredient in ingredients)
            {
                var audit = todayAudits.FirstOrDefault(x => x.IngredientId == ingredient.Id);

                decimal openingStock = 0;

                if (audit != null)
                {
                    openingStock = audit.OpeningStock;
                }
                else
                {
                    var previousDay = await stockAuditService.GetByIngredientAndDateAsync(ingredient.Id, businessDate.AddDays(-1));
                    if (previousDay != null)
                    {
                        openingStock = previousDay.ActualClosingStock;
                    }
                }

                model.Add(new StockReconciliationRow
                {
                    IngredientId = ingredient.Id,
                    IngredientName = ingredient.Name,
                    UnitSymbol = ingredient.Unit?.UnitSymbol,
                    Tolerance = ingredient.Tolerance,
                    OpeningStock = openingStock,
                    ActualClosingStock = audit?.ActualClosingStock ?? 0
                });
            }

            ViewBag.BusinessDate = businessDate;
            return View(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> BulkUpdate(List<Guid> ingredientIds, List<decimal> actualClosingStocks, DateTime businessDate)
        {
            // 1. Prevent future updates
            if (businessDate > GetBusinessDate(DateTime.Now))
            {
                TempData["Error"] = "You cannot update future stock counts.";
                return RedirectToAction(nameof(Index));
            }

            // 2. Prevent negative stock
            for (int i = 0; i < actualClosingStocks.Count; i++)
            {
                if (actualClosingStocks[i] < 0)
                {
                    TempData["Error"] = "Stock cannot be negative. Please check your input.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ingredientIds == null || ingredientIds.Count == 0)
            {
                TempData["Error"] = "No ingredients selected.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                for (int i = 0; i < ingredientIds.Count; i++)
                {
                    var ingredientId = ingredientIds[i];
                    var closingStock = actualClosingStocks[i];

                    var existingAudit = await stockAuditService.GetByIngredientAndDateAsync(ingredientId, businessDate);

                    if (existingAudit == null)
                    {
                        var previousDay = await stockAuditService.GetByIngredientAndDateAsync(ingredientId, businessDate.AddDays(-1));
                        var openingStock = previousDay?.ActualClosingStock ?? 0;

                        await stockAuditService.RegisterStockAuditAsync(new StockAudit
                        {
                            Id = Guid.NewGuid(),
                            IngredientId = ingredientId,
                            OpeningStock = openingStock,
                            ActualClosingStock = closingStock,
                            AuditDate = businessDate
                        });
                    }
                    else
                    {
                        existingAudit.ActualClosingStock = closingStock;
                        existingAudit.UpdatedAt = DateTime.UtcNow;
                        await stockAuditService.UpdateStockAuditAsync(existingAudit);
                    }
                }

                TempData["Success"] = "Stock updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> History(DateTime? date)
        {
            var businessDate = date ?? GetBusinessDate(DateTime.Now);
            var audits = await stockAuditService.GetByDateAsync(businessDate);

            ViewBag.BusinessDate = businessDate;
            return View(audits);
        }

        // 5-Hour Buffer Logic
        private DateTime GetBusinessDate(DateTime input)
        {
            if (input.Hour < 5)
            {
                return input.Date.AddDays(-1);
            }
            return input.Date;
        }
    }

    public class StockReconciliationRow
    {
        public Guid IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string? UnitSymbol { get; set; }
        public decimal Tolerance { get; set; }
        public decimal OpeningStock { get; set; }
        public decimal ActualClosingStock { get; set; }
    }
}