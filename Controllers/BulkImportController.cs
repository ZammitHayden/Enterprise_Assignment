using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Domain.Interfaces;
using Enterprise_Assignment.Models;

namespace Enterprise_Assignment.Controllers
{
    public class BulkImportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Preview()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BulkImport(
            string jsonData,
            [FromKeyedServices("InMemory")] IItemsRepository inMemoryRepository)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                ViewBag.Error = "No JSON data provided.";
                return View("Index");
            }

            try
            {
                var factory = new ImportItemFactory();
                List<IItemValidating> items = factory.Create(jsonData);

                foreach (var item in items)
                {
                    if (item is Restaurant restaurant)
                    {
                        restaurant.Status = "Pending";
                    }
                    else if (item is MenuItem menuItem)
                    {
                        menuItem.Status = "Pending";
                    }
                }

                var sessionId = HttpContext.Session.Id;
                inMemoryRepository.SaveItems(sessionId, items);

                return View("Preview", items);
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = $"Error processing JSON: {ex.Message}";
                return View("Index");
            }
        }

        [HttpPost]
        public IActionResult ProcessImport(
            [FromKeyedServices("InMemory")] IItemsRepository inMemoryRepository,
            [FromKeyedServices("Database")] IItemsRepository dbRepository)
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var items = inMemoryRepository.GetItems(sessionId);

                if (items == null || items.Count == 0)
                {
                    TempData["Error"] = "No items found to import. Please upload JSON data first.";
                    return RedirectToAction("Index");
                }

                dbRepository.SaveItems(sessionId, items);

                inMemoryRepository.ClearItems(sessionId);

                TempData["Message"] = $"Successfully processed {items.Count} items for import.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error during import: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}