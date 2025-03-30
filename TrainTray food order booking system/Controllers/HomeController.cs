using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TrainTray_food_order_booking_system.Models; // Add your models namespace
using System.Threading.Tasks;
using BCrypt.Net;
namespace TrainTray_food_order_booking_system.Controllers
{
    public class HomeController : Controller
    {
        private readonly TblUserContext _context; // Inject your database context
        public IActionResult AccessDenied()
        {
            return View();
        }

        public HomeController(TblUserContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            
            var userId = HttpContext.Session.GetString("UserID");
            var userType = HttpContext.Session.GetString("UserType");

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userType))
            {
                Console.WriteLine($"Registration: UserType = {userType}");

                if (userType == "1")
                {
                    Console.WriteLine("Redirecting to RestaurantDashboard");
                    return RedirectToAction("RestaurantDashboard", "Restaurant");
                }
                // ❌ Don't redirect to Dashboard again!
            }
            else
            {
                Console.WriteLine("Session values are missing: UserID or UserType is NULL");
            }





        ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["ErrorMessage"] = "All fields are required.";
                return RedirectToAction("Dashboard");
            }

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirm password do not match.";
                return RedirectToAction("Dashboard");
            }

            // Get User ID from session
            string userId = HttpContext.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User not logged in.";
                return RedirectToAction("Login", "Login");
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Dashboard");
            }

            // Verify current password
            if (string.IsNullOrEmpty(user.Password) || !BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                return RedirectToAction("Dashboard");
            }

            // Hash the new password
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password updated successfully!";
            return RedirectToAction("Dashboard");
        }
    }
}
