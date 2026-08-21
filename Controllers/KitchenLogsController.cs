using Microsoft.AspNetCore.Mvc;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Controllers
{
    public class KitchenLogsController : Controller
    {
        private readonly IKitchenLogService kitchenLogService;
        private readonly IProductService productService;
        private readonly IIngredientService ingredientService;
        private readonly IStockAuditService stockAuditService;
        private readonly IStockInService stockInService;

        public KitchenLogsController(
            IKitchenLogService _kitchenLogService,
            IProductService _productService,
            IIngredientService _ingredientService,
            IStockAuditService _stockAuditService,
            IStockInService _stockInService)
        {
            kitchenLogService = _kitchenLogService;
            productService = _productService;
            ingredientService = _ingredientService;
            stockAuditService = _stockAuditService;
            stockInService = _stockInService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var logs = await kitchenLogService.GetAllAsync();
            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var products = await productService.GetAllAsync();
            ViewData["ProductList"] = products;
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(List<Guid> productIds, List<int> quantities)
        {
            if (productIds == null || productIds.Count == 0)
            {
                TempData["Error"] = "Please select at least one product.";
                return RedirectToAction(nameof(Create));
            }

            try
            {
                var allProducts = await productService.GetAllAsync();
                var allIngredients = await ingredientService.GetAllAsync();
                var businessDate = DateTime.Now.Date;
                var todayLogs = await kitchenLogService.GetAllAsync();
                var todaysLogs = todayLogs.Where(k => k.DateLogged.Date == businessDate.Date).ToList();

                // 🔥 VALIDATION LOOP
                for (int i = 0; i < productIds.Count; i++)
                {
                    var productId = productIds[i];
                    var quantityToMake = quantities[i];

                    if (quantityToMake <= 0) continue;

                    var product = allProducts.FirstOrDefault(p => p.Id == productId);
                    if (product == null) continue;

                    // Check ALL ingredients for this product
                    foreach (var recipe in product.ProductIngredients)
                    {
                        var ingredientId = recipe.IngredientId;
                        var requiredQuantity = recipe.QuantityRequired * quantityToMake;

                        // Calculate current stock
                        var audit = await stockAuditService.GetByIngredientAndDateAsync(ingredientId, businessDate);
                        var openingStock = audit?.ActualClosingStock ?? 0;
                        var stockIn = await stockInService.GetByDateAsync(businessDate);
                        var totalStockIn = stockIn.Where(s => s.IngredientId == ingredientId).Sum(s => s.QuantityReceived);

                        decimal usedSoFar = 0;
                        foreach (var log in todaysLogs)
                        {
                            var prod = allProducts.FirstOrDefault(p => p.Id == log.ProductId);
                            if (prod != null)
                            {
                                var rec = prod.ProductIngredients.FirstOrDefault(pi => pi.IngredientId == ingredientId);
                                if (rec != null)
                                {
                                    usedSoFar += rec.QuantityRequired * log.QuantityMade;
                                }
                            }
                        }

                        var available = openingStock + totalStockIn - usedSoFar;

                        // 🚨 BLOCK if not enough stock
                        if (requiredQuantity > available)
                        {
                            var ingredientName = allIngredients.FirstOrDefault(ing => ing.Id == ingredientId)?.Name;
                            TempData["Error"] = $"Not enough stock for {product.Name}. Need {requiredQuantity} of {ingredientName}, but only {available} available.";
                            return RedirectToAction(nameof(Create));
                        }
                    }
                }

                // ✅ If all checks pass, save the logs
                await kitchenLogService.LogMultiAsync(productIds, quantities);
                TempData["Success"] = "Kitchen logs added successfully!";
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
            var log = await kitchenLogService.GetByIdAsync(id);
            if (log == null)
                return NotFound();

            var products = await productService.GetAllAsync();
            ViewData["ProductList"] = products;
            return View(log);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Edit(Guid Id, KitchenLog log)
        {
            if (Id != log.Id)
            {
                TempData["Error"] = "Invalid log ID.";
                return RedirectToAction(nameof(Index));
            }

            if (log.QuantityMade <= 0)
            {
                TempData["Error"] = "Quantity must be greater than 0.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var existing = await kitchenLogService.GetByIdAsync(Id);
                if (existing == null)
                {
                    TempData["Error"] = "Log not found.";
                    return RedirectToAction(nameof(Index));
                }

                // 🚨 STOCK ADJUSTMENT ON EDIT (If Qty Changes)
                if (existing.QuantityMade != log.QuantityMade)
                {
                    // Get old and new product recipes
                    var oldProduct = await productService.GetByIdAsync(existing.ProductId);
                    var newProduct = await productService.GetByIdAsync(log.ProductId);
                    var businessDate = DateTime.Now.Date;

                    // 🔥 ADJUST STOCK FOR INGREDIENTS
                    if (oldProduct != null)
                    {
                        foreach (var recipe in oldProduct.ProductIngredients)
                        {
                            // If product changed, return old stock
                            if (existing.ProductId != log.ProductId)
                            {
                                var oldQty = existing.QuantityMade;
                                var stockToReturn = recipe.QuantityRequired * oldQty;
                                var audit = await stockAuditService.GetByIngredientAndDateAsync(recipe.IngredientId, businessDate);
                                if (audit != null)
                                {
                                    audit.ActualClosingStock += stockToReturn; // Add back stock
                                    await stockAuditService.UpdateStockAuditAsync(audit);
                                }
                            }
                            // If same product but qty decreased, return excess stock
                            else if (log.QuantityMade < existing.QuantityMade)
                            {
                                var diff = existing.QuantityMade - log.QuantityMade;
                                var stockToReturn = recipe.QuantityRequired * diff;
                                var audit = await stockAuditService.GetByIngredientAndDateAsync(recipe.IngredientId, businessDate);
                                if (audit != null)
                                {
                                    audit.ActualClosingStock += stockToReturn; // Add back stock
                                    await stockAuditService.UpdateStockAuditAsync(audit);
                                }
                            }
                            // If same product but qty increased, check & deduct stock
                            else if (log.QuantityMade > existing.QuantityMade)
                            {
                                var diff = log.QuantityMade - existing.QuantityMade;
                                var requiredExtraStock = recipe.QuantityRequired * diff;

                                var stockAudit = await stockAuditService.GetByIngredientAndDateAsync(recipe.IngredientId, businessDate);
                                var available = stockAudit?.ActualClosingStock ?? 0;
                                var stockIn = await stockInService.GetByDateAsync(businessDate);
                                var totalStockIn = stockIn.Where(s => s.IngredientId == recipe.IngredientId).Sum(s => s.QuantityReceived);
                                var fullAvailable = available + totalStockIn;

                                if (requiredExtraStock > fullAvailable)
                                {
                                    var ingredientName = (await ingredientService.GetByIdAsync(recipe.IngredientId))?.Name;
                                    TempData["Error"] = $"Not enough stock for {newProduct.Name}. Need {requiredExtraStock} more of {ingredientName}.";
                                    return RedirectToAction(nameof(Index));
                                }

                                if (stockAudit != null)
                                {
                                    stockAudit.ActualClosingStock -= requiredExtraStock;
                                    await stockAuditService.UpdateStockAuditAsync(stockAudit);
                                }
                            }
                        }
                    }

                    // If product changed, add new product's stock deduction
                    if (existing.ProductId != log.ProductId && newProduct != null)
                    {
                        foreach (var recipe in newProduct.ProductIngredients)
                        {
                            var qtyToDeduct = recipe.QuantityRequired * log.QuantityMade;
                            var audit = await stockAuditService.GetByIngredientAndDateAsync(recipe.IngredientId, businessDate);
                            if (audit != null)
                            {
                                audit.ActualClosingStock -= qtyToDeduct;
                                await stockAuditService.UpdateStockAuditAsync(audit);
                            }
                        }
                    }
                }

                existing.ProductId = log.ProductId;
                existing.QuantityMade = log.QuantityMade;
                existing.DateLogged = DateTime.Now;

                await kitchenLogService.UpdateLogAsync(existing);
                TempData["Success"] = "Log updated successfully!";
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
                var deleted = await kitchenLogService.DeleteLogAsync(id);
                if (deleted)
                {
                    TempData["Success"] = "Log deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Log not found.";
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