using Microsoft.AspNetCore.Mvc;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Controllers
{
    public class KitchenLogsController : Controller
    {
        private readonly IKitchenLogService kitchenLogService;
        private readonly IProductService productService;

        public KitchenLogsController(IKitchenLogService _kitchenLogService, IProductService _productService)
        {
            kitchenLogService = _kitchenLogService;
            productService = _productService;
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