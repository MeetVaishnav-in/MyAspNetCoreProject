using Microsoft.AspNetCore.Mvc;
using TrainTray_food_order_booking_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;
using TrainTray_food_order_booking_system.Models;

namespace TrainTray_food_order_booking_system.Controllers
{
    public class LoginController : Controller
    {
        private readonly TblUserContext _context;

        public LoginController(TblUserContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
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
                else if (userType == "0")
                {
                    Console.WriteLine("Redirecting to Dashboard");
                    return RedirectToAction("Index", "Train");

                }
            }
            else
            {
                Console.WriteLine("Session values are missing: UserID or UserType is NULL");
            }
            



            return View();
              
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
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
                else if (userType == "0")
                {
                    Console.WriteLine("Redirecting to Dashboard");
                    return RedirectToAction("Index", "Train");
                }
            }
            else
            {
                Console.WriteLine("Session values are missing: UserID or UserType is NULL 2");
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (model.Email == "admin12@gmail.com" && model.Password == "1234")
            {
                HttpContext.Session.SetString("UserID", "9999"); // Static ID for admin
                HttpContext.Session.SetString("UserName", "Admin");
                HttpContext.Session.SetString("UserType", "admin");

                Console.WriteLine("Admin logged in successfully!");
                return RedirectToAction("AdminPage", "Admin"); // Redirect to AdminPage
            }
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // Check if the user is blocked
            var failedAttempt = await _context.FailedLoginAttempts.FirstOrDefaultAsync(f => f.Email == model.Email);

            if (failedAttempt != null && failedAttempt.BlockedUntil.HasValue && failedAttempt.BlockedUntil > DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Your account is locked. Please try again later.");
                return View(model);
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                if (failedAttempt == null)
                {
                    failedAttempt = new FailedLoginAttempt
                    {
                        Email = model.Email,
                        AttemptCount = 1,
                        BlockedUntil = null
                    };
                    await _context.FailedLoginAttempts.AddAsync(failedAttempt);
                }
                else
                {
                    failedAttempt.AttemptCount++;

                    if (failedAttempt.AttemptCount >= 3)
                    {
                        failedAttempt.BlockedUntil = DateTime.UtcNow.AddMinutes(5); // Block for 5 minutes
                        ModelState.AddModelError("", "Too many failed attempts. Try again in 5 minutes.");
                    }
                }

                await _context.SaveChangesAsync();
                return View(model);
            }

            // Reset failed attempts on successful login
            if (failedAttempt != null)
            {
                _context.FailedLoginAttempts.Remove(failedAttempt);
                await _context.SaveChangesAsync();
            }

            

            if (user.Type.ToString() == "1")
            {
                HttpContext.Session.SetString("UserID", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserType", user.Type.ToString());
                
                return RedirectToAction("Register", "Restaurant");
            }
            else if(user.Type.ToString() == "0")
            {
                HttpContext.Session.SetString("UserID", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserType", user.Type.ToString());
                Console.WriteLine("d2");
                return RedirectToAction("Index", "Train");
            }
            return View(model);

        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();  
            return RedirectToAction("Login");
        }

    }
}
