using Microsoft.AspNetCore.Identity;

namespace Enterprise_Assignment.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsSiteAdmin { get; set; }
        public virtual ICollection<Restaurant> OwnedRestaurants { get; set; }
    }
}