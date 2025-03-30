using Microsoft.AspNetCore.Mvc;
using TrainTray_food_order_booking_system.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System;
using System.Threading.Tasks;
using TrainTray_food_order_booking_system.Services;

namespace TrainTray_food_order_booking_system.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly TblUserContext _context;
        private readonly EmailService _emailService;
        private readonly GoogleRecaptchaService _recaptchaService;
        public RegistrationController(TblUserContext context, EmailService emailService, GoogleRecaptchaService recaptchaService)
        {
            _context = context;
            _emailService = emailService;
            _recaptchaService = recaptchaService;
        }

        [HttpGet]
        public IActionResult RegistrationForm()
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
                    return RedirectToAction("Dashboard", "Home");
                }
            }
            else
            {
                Console.WriteLine("Session values are missing: UserID or UserType is NULL");
            }



            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrationForm(User model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var existingUser1 = await _context.User.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser1 != null)
            {
                ModelState.AddModelError("", "Email already exists. Please use another email.");
                return View(model);
            }

            // Generate OTP
            string otp = new Random().Next(100000, 999999).ToString();
            var userDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            HttpContext.Session.SetString("UserData", userDataJson);
            // Send OTP Email
            bool emailSent = await _emailService.SendOtpEmailAsync(model.Email, otp);

            if (!emailSent)
            {
            Console.WriteLine("oof");
                ModelState.AddModelError("", "Failed to send OTP. Please try again.");
                return View(model);
            }

            // Check if email already exists in tblTemp
            var existingUser = await _context.TempUsers.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (existingUser == null)
            {
                // Insert new OTP entry
                var tempUser = new TempUser
                {
                    Email = model.Email,
                    Otp = otp,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.TempUsers.AddAsync(tempUser);
            }
            else
            {
                // Update OTP for existing email
                existingUser.Otp = otp;
                existingUser.CreatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("VerifyOtp", "Registration", new { email = model.Email });
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            var tempUser = await _context.TempUsers.FirstOrDefaultAsync(u => u.Email == email);

            if (tempUser == null || tempUser.Otp != otp)
            {
                Console.WriteLine("wrog");
                ModelState.AddModelError("", "Invalid OTP. Please try again.");
                return View();
            }

           
            if (tempUser.CreatedAt < DateTime.UtcNow.AddMinutes(-5))
            {
                Console.WriteLine("timw");
                ModelState.AddModelError("", "OTP has expired. Please request a new OTP.");
                return View();
            }

            var userDataJson = HttpContext.Session.GetString("UserData");
            if (string.IsNullOrEmpty(userDataJson))
            {
                Console.WriteLine("registration");
                ModelState.AddModelError("", "Session expired. Please register again.");
                return RedirectToAction("RegistrationForm");
            }

            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(userDataJson);

            var newUser = new User()
            {
                Name = model.Name,
                Email = model.Email,
                Mobile = model.Mobile,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password), 
                Type = model.Type
            };

            await _context.User.AddAsync(newUser);
            await _context.SaveChangesAsync();

            _context.TempUsers.Remove(tempUser);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("UserData");
            if (model.Type.Equals('1'))
            {
                Console.WriteLine("d");
                return RedirectToAction("RestaurantRegistrationForm", "Restaurant");
            }
            TempData["SuccessMessage"] = "Registration successful! You can now log in.";
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Invalid email.";
                return RedirectToAction("VerifyOtp", new { email });
            }

            string newOtp = new Random().Next(100000, 999999).ToString();

            // Send OTP Email
            bool emailSent = await _emailService.SendOtpEmailAsync(email, newOtp);
            if (!emailSent)
            {
                TempData["ErrorMessage"] = "Failed to resend OTP. Please try again.";
                return RedirectToAction("VerifyOtp", new { email });
            }

            // Update OTP in tblTemp
            var existingUser = await _context.TempUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                existingUser.Otp = newOtp;
                existingUser.CreatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }



            TempData["SuccessMessage"] = "OTP has been resent successfully!";
            return RedirectToAction("VerifyOtp", new { email });
        }


    }
}
