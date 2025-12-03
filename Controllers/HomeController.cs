using Domain.Interfaces;
using Enterprise_Assignment.Data.Repositories;
using Enterprise_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Enterprise_Assignment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ItemsDbRepository _dbRepository;
        private const string SessionKey = "LoggedInUser";

        public HomeController(ILogger<HomeController> logger, ItemsDbRepository dbRepository)
        {
            _logger = logger;
            _dbRepository = dbRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog(string view = "card")
        {
            var items = _dbRepository.GetApprovedItems();
            ViewBag.ViewType = view;
            ViewBag.IsApprovalPage = false;
            return View(items);
        }

        public IActionResult RestaurantMenu(int restaurantId)
        {
            var menuItems = _dbRepository.GetApprovedMenuItems(restaurantId);

            var restaurant = _dbRepository.GetRestaurantById(restaurantId);

            ViewBag.Restaurant = restaurant;
            return View(menuItems);
        }

        private bool IsUserLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKey));
        }

        [HttpGet]
        public IActionResult PendingApproval(string view = "card")
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var userElement = JsonSerializer.Deserialize<JsonElement>(HttpContext.Session.GetString(SessionKey));
            var isSiteAdmin = false;
            if (userElement.TryGetProperty("IsSiteAdmin", out var isAdminElement))
            {
                isSiteAdmin = isAdminElement.GetBoolean();
            }

            if (!isSiteAdmin)
            {
                return RedirectToAction("Verification");
            }

            var items = _dbRepository.GetPendingItems();
            ViewBag.ViewType = view;
            ViewBag.IsApprovalPage = true;
            return View("Catalog", items);
        }

        private dynamic GetCurrentUser()
        {
            var userJson = HttpContext.Session.GetString(SessionKey);
            if (!string.IsNullOrEmpty(userJson))
            {
                return JsonSerializer.Deserialize<dynamic>(userJson);
            }
            return null;
        }

        [HttpPost]
        public IActionResult ApproveItems(string[] selectedItems)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (selectedItems == null || selectedItems.Length == 0)
            {
                TempData["Error"] = "No items selected for approval.";
                return RedirectToAction("Verification");
            }

            foreach (var itemId in selectedItems)
            {
                if (!string.IsNullOrEmpty(itemId))
                {
                    _dbRepository.Approve(itemId);
                }
            }

            TempData["Message"] = $"Successfully approved {selectedItems.Length} item(s)";
            return RedirectToAction("Verification");
        }

        [HttpGet]
        public IActionResult Verification()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = GetCurrentUser();
            var isSiteAdmin = false;

            var userElement = JsonSerializer.Deserialize<JsonElement>(HttpContext.Session.GetString(SessionKey));
            if (userElement.TryGetProperty("IsSiteAdmin", out var isAdminElement))
            {
                isSiteAdmin = isAdminElement.GetBoolean();
            }

            if (isSiteAdmin)
            {
                return RedirectToAction("PendingApproval");
            }
            else
            {
                var userEmail = "";
                if (userElement.TryGetProperty("Email", out var emailElement))
                {
                    userEmail = emailElement.GetString();
                }

                var ownedRestaurants = _dbRepository.GetRestaurantsByOwner(userEmail);
                var ownedRestaurantIds = ownedRestaurants.Select(r => r.Id).ToList();

                var pendingMenuItems = _dbRepository.GetPendingMenuItems()
                    .Where(m => ownedRestaurantIds.Contains(m.RestaurantFK))
                    .Cast<IItemValidating>()
                    .ToList();

                var pendingOwnedRestaurants = _dbRepository.GetPendingRestaurants()
                    .Where(r => r.OwnerEmailAddress == userEmail)
                    .Cast<IItemValidating>()
                    .ToList();

                var allPendingItems = pendingOwnedRestaurants.Concat(pendingMenuItems).ToList();

                ViewBag.ViewType = "card";
                ViewBag.IsApprovalPage = true;
                ViewBag.UserEmail = userEmail;

                return View("Catalog", allPendingItems);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}