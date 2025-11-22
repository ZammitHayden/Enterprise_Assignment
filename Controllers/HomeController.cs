using System.Diagnostics;
using Domain.Interfaces;
using Enterprise_Assignment.Models;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise_Assignment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog(string view = "card")
        {
            var items = GetMixedItems();
            ViewBag.ViewType = view;
            ViewBag.IsApprovalPage = false;
            return View(items);
        }

        public IActionResult PendingApproval(string view = "card")
        {
            var items = GetPendingItems();
            ViewBag.ViewType = view;
            ViewBag.IsApprovalPage = true;
            return View("Catalog", items);
        }

        [HttpPost]
        public IActionResult ApproveItems(string[] selectedItems)
        {
            foreach (var itemId in selectedItems)
            {
                if (!string.IsNullOrEmpty(itemId))
                {
                    ApproveItem(itemId);
                }
            }
            return RedirectToAction("PendingApproval");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Mock data methods 
        private IEnumerable<IItemValidating> GetMixedItems()
        {
            var items = new List<IItemValidating>();

            // Sample restaurants
            items.Add(new Restaurant
            {
                Id = 1,
                Name = "Trattoria Luca",
                //Description = "Pasta & grill with fresh daily specials",
                OwnerEmailAddress = "luca.owner@example.com",
                // Address = "123 Harbor Road, Valletta",
                //Phone = "+356 1234 5678",
                Status = "Approved"
            });

            items.Add(new Restaurant
            {
                Id = 2,
                Name = "Sushi Wave",
                // Description = "Classic nigiri and creative rolls",
                OwnerEmailAddress = "hana.owner@example.com",
                // Address = "45 Marina Street, Sliema",
                //Phone = "+356 9876 5432",
                Status = "Pending"
            });

            // Sample menu items
            var restaurant1 = new Restaurant { Id = 1, OwnerEmailAddress = "luca.owner@example.com" };

            items.Add(new MenuItem
            {
                Id = Guid.NewGuid(),
                Title = "Tagliatelle al Ragù",
                Price = 11.50f,
                //Currency = "EUR",
                Restaurant = restaurant1,
                RestaurantFK = 1,
                Status = "Approved"
            });

            items.Add(new MenuItem
            {
                Id = Guid.NewGuid(),
                Title = "Ribeye 300g",
                Price = 24.00f,
                //Currency = "EUR",
                Restaurant = restaurant1,
                RestaurantFK = 1,
                Status = "Pending"
            });

            return items;
        }

        private IEnumerable<IItemValidating> GetPendingItems()
        {
            var allItems = GetMixedItems();
            return allItems.Where(item =>
                (item is Restaurant r && r.Status == "Pending") ||
                (item is MenuItem m && m.Status == "Pending")
            );
        }

        private void ApproveItem(string itemId)
        {

            var parts = itemId.Split('-');
            if (parts.Length == 2)
            {
                var type = parts[0];
                var id = parts[1];

                _logger.LogInformation($"Approving {type} with ID: {id}");
            }
        }
    }
}
