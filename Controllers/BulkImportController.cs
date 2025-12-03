using Microsoft.AspNetCore.Mvc;
using Domain.Interfaces;
using Enterprise_Assignment.Models;
using System.IO.Compression;
using Enterprise_Assignment.Services;

namespace Enterprise_Assignment.Controllers
{
    public class BulkImportController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public BulkImportController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

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
                        restaurant.ImageUrl = "/Assets/default.png";
                    }
                    else if (item is MenuItem menuItem)
                    {
                        menuItem.Status = "Pending";
                        menuItem.ImageUrl = "/Assets/default.png";
                    }
                }

                var sessionId = HttpContext.Session.Id;
                inMemoryRepository.SaveItems(sessionId, items);

                var zipBytes = GenerateImageTemplateZip(items);
                HttpContext.Session.Set("TemplateZip", zipBytes);

                ViewBag.ItemsCount = items.Count;
                ViewBag.HasTemplateZip = true;

                return View("Preview", items);
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = $"Error processing JSON: {ex.Message}";
                return View("Index");
            }
        }

        public IActionResult DownloadTemplate()
        {
            var zipBytes = HttpContext.Session.Get("TemplateZip") as byte[];
            if (zipBytes == null)
            {
                TempData["Error"] = "No template available. Please import JSON data first.";
                return RedirectToAction("Index");
            }

            return File(zipBytes, "application/zip", "image-template.zip");
        }

        [HttpPost]
        public IActionResult CommitImport(IFormFile zipFile,
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

                if (zipFile != null && zipFile.Length > 0)
                {
                    ProcessUploadedImages(zipFile, items);
                }

                dbRepository.SaveItems(sessionId, items);
                inMemoryRepository.ClearItems(sessionId);
                HttpContext.Session.Remove("TemplateZip");

                TempData["Message"] = $"Successfully imported {items.Count} items with images.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error during import: {ex.Message}";
                return RedirectToAction("Index");
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
                HttpContext.Session.Remove("TemplateZip");

                TempData["Message"] = $"Successfully processed {items.Count} items for import.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error during import: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private byte[] GenerateImageTemplateZip(List<IItemValidating> items)
        {
            using var memoryStream = new MemoryStream();

            var placeholderPath = Path.Combine(_environment.WebRootPath, "Assets", "default.png");

            if (!System.IO.File.Exists(placeholderPath))
            {
                throw new FileNotFoundException($"Placeholder image not found at: {placeholderPath}");
            }

            var placeholderImage = System.IO.File.ReadAllBytes(placeholderPath);

            using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var item in items)
                {
                    var itemId = item switch
                    {
                        Restaurant r => r.Id.ToString(),
                        MenuItem m => m.Id.ToString(),
                        _ => Guid.NewGuid().ToString()
                    };

                    var folderName = $"item-{itemId}";
                    var entry = zipArchive.CreateEntry($"{folderName}/default.png", CompressionLevel.Fastest);

                    using var entryStream = entry.Open();
                    entryStream.Write(placeholderImage, 0, placeholderImage.Length);
                }
            }

            return memoryStream.ToArray();
        }

        private void ProcessUploadedImages(IFormFile zipFile, List<IItemValidating> items)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "images", "uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            using var zipStream = zipFile.OpenReadStream();
            using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);

            foreach (var entry in zipArchive.Entries)
            {
                if (entry.FullName.EndsWith("/default.png", StringComparison.OrdinalIgnoreCase) &&
                    entry.Length > 0)
                {
                    var folderName = Path.GetDirectoryName(entry.FullName);
                    var itemId = folderName?.Replace("item-", "");

                    if (!string.IsNullOrEmpty(itemId))
                    {
                        var item = items.FirstOrDefault(i =>
                            (i is Restaurant r && r.Id.ToString() == itemId) ||
                            (i is MenuItem m && m.Id.ToString() == itemId));

                        if (item != null)
                        {
                            var uniqueFileName = $"{Guid.NewGuid()}.png";
                            var filePath = Path.Combine(uploadsPath, uniqueFileName);

                            using var entryStream = entry.Open();
                            using var fileStream = new FileStream(filePath, FileMode.Create);
                            entryStream.CopyTo(fileStream);

                            var imageUrl = $"/images/uploads/{uniqueFileName}";

                            if (item is Restaurant restaurant)
                                restaurant.ImageUrl = imageUrl;
                            else if (item is MenuItem menuItem)
                                menuItem.ImageUrl = imageUrl;
                        }
                    }
                }
            }
        }
    }
}