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

        public void SaveItems(string sessionId, List<IItemValidating> items)
        {
            foreach (var item in items)
            {
                if (item is Restaurant restaurant)
                {
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
                else if (item is MenuItem menuItem)
                {
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
            }

            _context.SaveChanges();
        }

        public void ClearItems(string sessionId)
        {
            throw new NotImplementedException("This method is not supported for database repository");
        }
    }
}