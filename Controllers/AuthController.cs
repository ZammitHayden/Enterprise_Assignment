using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Enterprise_Assignment.Controllers
{
    public class AuthController : Controller
    {
        private const string SessionKey = "LoggedInUser";
        private const string AdminEmail = "admin@enterprise.com";
        private const string AdminPassword = "Admin@123";

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (email == AdminEmail && password == AdminPassword)
            {
                var userData = new { Email = email, IsSiteAdmin = true, Role = "SiteAdmin" };
                HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(userData));
                return RedirectToAction("Verification", "Home");
            }
            else if (!string.IsNullOrEmpty(email))
            {
                var userData = new { Email = email, IsSiteAdmin = false, Role = "RestaurantOwner" };
                HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(userData));
                return RedirectToAction("Verification", "Home");
            }

            ViewBag.Error = "Please enter an email";
            return View();
        }

        [HttpPost]
        public IActionResult Register(string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View();
            }

            var userData = new { Email = email, IsSiteAdmin = false, Role = "RestaurantOwner" };
            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(userData));
            return RedirectToAction("Verification", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove(SessionKey);
            return RedirectToAction("Login");
        }

        public static dynamic GetCurrentUser(ISession session)
        {
            var userJson = session.GetString(SessionKey);
            if (!string.IsNullOrEmpty(userJson))
            {
                return JsonSerializer.Deserialize<dynamic>(userJson);
            }
            return null;
        }
    }
}
