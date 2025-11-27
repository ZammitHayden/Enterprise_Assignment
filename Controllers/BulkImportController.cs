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
        public IActionResult BulkImport(string jsonData)
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

                return View("Preview", items);
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = $"Error processing JSON: {ex.Message}";
                return View("Index");
            }
        }

        [HttpPost]
        public IActionResult ProcessImport(List<IItemValidating> items)
        {
            // This will be implemented in AA4.3 to save to database
            // For now, just redirect back with success message
            TempData["Message"] = $"Successfully processed {items?.Count ?? 0} items for import.";
            return RedirectToAction("Index");
        }
    }
}