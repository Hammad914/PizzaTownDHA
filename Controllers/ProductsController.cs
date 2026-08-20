using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService productService;
        private readonly IIngredientService ingredientService;

        public ProductsController(IProductService _productService, IIngredientService _ingredientService)
        {
            productService = _productService;
            ingredientService = _ingredientService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await productService.GetAllAsync();
            return View(products);
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
        public async Task<IActionResult> Create(Product product, List<Guid> selectedIngredientIds, List<decimal> quantities)
        {
            if (await productService.ProductNameExistsAsync(product.Name))
            {
                TempData["Error"] = "A product with this name already exists.";
                return RedirectToAction(nameof(Index));
            }

            if (selectedIngredientIds == null || selectedIngredientIds.Count == 0)
            {
                TempData["Error"] = "Please select at least one ingredient for this product.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await productService.RegisterProductAsync(product, selectedIngredientIds, quantities);
                    TempData["Success"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "A product with this name already exists.";
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["Error"] = "Please fill in all required fields correctly.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            var ingredients = await ingredientService.GetAllAsync();
            ViewData["IngredientList"] = ingredients;
            return View(product);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Edit(Guid Id, Product product, List<Guid> selectedIngredientIds, List<decimal> quantities)
        {
            if (Id != product.Id)
                return NotFound();

            if (await productService.ProductNameExistsAsync(product.Name, product.Id))
            {
                TempData["Error"] = "A product with this name already exists.";
                return RedirectToAction(nameof(Index));
            }

            if (selectedIngredientIds == null || selectedIngredientIds.Count == 0)
            {
                TempData["Error"] = "Please select at least one ingredient.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await productService.UpdateProductAsync(product, selectedIngredientIds, quantities);
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "A product with this name already exists.";
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
                var deleted = await productService.DeleteProductAsync(id);
                if (deleted)
                {
                    TempData["Success"] = "Product deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Product not found or could not be deleted.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = "An error occurred while deleting the product.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}