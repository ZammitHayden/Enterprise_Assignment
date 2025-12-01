using Domain.Interfaces;
using Enterprise_Assignment.Data.Repositories;
using Enterprise_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Enterprise_Assignment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ItemsDbRepository _dbRepository;

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

        public IActionResult PendingApproval(string view = "card")
        {
            var items = _dbRepository.GetPendingItems();
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
                    _dbRepository.ApproveItem(itemId);
                }
            }
            return RedirectToAction("PendingApproval");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}