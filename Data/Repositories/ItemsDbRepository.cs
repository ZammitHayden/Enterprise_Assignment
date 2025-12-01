using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using Enterprise_Assignment.Models;
using Microsoft.EntityFrameworkCore;

namespace Enterprise_Assignment.Data.Repositories
{
    public class ItemsDbRepository : IItemsRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemsDbRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<IItemValidating> GetItems(string sessionId)
        {
            throw new NotImplementedException("This method is not supported for database repository in this context");
        }

        public List<IItemValidating> GetApprovedRestaurants()
        {
            var restaurants = _context.Restaurants
                .Where(r => r.Status == "Approved")
                .ToList();

            return restaurants.Cast<IItemValidating>().ToList();
        }

        public List<MenuItem> GetApprovedMenuItems(int restaurantId)
        {
            return _context.MenuItems
                .Where(m => m.RestaurantFK == restaurantId && m.Status == "Approved")
                .ToList();
        }

        public List<IItemValidating> GetApprovedItems()
        {
            var restaurants = _context.Restaurants
                .Where(r => r.Status == "Approved")
                .Cast<IItemValidating>()
                .ToList();

            var menuItems = _context.MenuItems
                .Where(m => m.Status == "Approved")
                .Cast<IItemValidating>()
                .ToList();

            return restaurants.Concat(menuItems).ToList();
        }

        public List<IItemValidating> GetPendingItems()
        {
            var pendingRestaurants = _context.Restaurants
                .Where(r => r.Status == "Pending")
                .Cast<IItemValidating>()
                .ToList();

            var pendingMenuItems = _context.MenuItems
                .Where(m => m.Status == "Pending")
                .Cast<IItemValidating>()
                .ToList();

            return pendingRestaurants.Concat(pendingMenuItems).ToList();
        }

        public Restaurant GetRestaurantById(int restaurantId)
        {
            return _context.Restaurants
                .FirstOrDefault(r => r.Id == restaurantId);
        }

        public void ApproveItem(string itemId)
        {
            var parts = itemId.Split('-');
            if (parts.Length == 2)
            {
                var type = parts[0];
                var id = parts[1];

                if (type == "Restaurant" && int.TryParse(id, out int restaurantId))
                {
                    var restaurant = _context.Restaurants.FirstOrDefault(r => r.Id == restaurantId);
                    if (restaurant != null)
                    {
                        restaurant.Status = "Approved";
                        _context.SaveChanges();
                    }
                }
                else if (type == "MenuItem" && Guid.TryParse(id, out Guid menuItemId))
                {
                    var menuItem = _context.MenuItems.FirstOrDefault(m => m.Id == menuItemId);
                    if (menuItem != null)
                    {
                        menuItem.Status = "Approved";
                        _context.SaveChanges();
                    }
                }
            }
        }

        public void SaveItems(string sessionId, List<IItemValidating> items)
        {
            foreach (var item in items.OfType<Restaurant>())
            {
                var restaurant = item;
                var existingRestaurant = _context.Restaurants
                    .FirstOrDefault(r => r.Id == restaurant.Id);

                if (existingRestaurant != null)
                {
                    _context.Entry(existingRestaurant).CurrentValues.SetValues(restaurant);
                }
                else
                {
                    _context.Restaurants.Add(restaurant);
                }
            }

            _context.SaveChanges();

            foreach (var item in items.OfType<MenuItem>())
            {
                var menuItem = item;
                var existingMenuItem = _context.MenuItems
                    .FirstOrDefault(m => m.Id == menuItem.Id);

                if (existingMenuItem != null)
                {
                    _context.Entry(existingMenuItem).CurrentValues.SetValues(menuItem);
                }
                else
                {
                    _context.MenuItems.Add(menuItem);
                }
            }

            _context.SaveChanges();
        }

        public void ClearItems(string sessionId)
        {
            throw new NotImplementedException("This method is not supported for database repository");
        }
    }
}