using Microsoft.AspNetCore.Mvc;
using TrainTray_food_order_booking_system.Models;
using TrainTray_food_order_booking_system.Services;

using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;

namespace TrainTray_food_order_booking_system.Controllers
{
    public class ForgotPasswordController : Controller
    {
        private readonly TblUserContext _context;
        private readonly EmailService _emailService;

        public ForgotPasswordController(TblUserContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email not found.");
                return View();
            }

            // Generate OTP
            string otp = new Random().Next(100000, 999999).ToString();

            // Save OTP to temp table
            var tempUser = await _context.TempUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (tempUser == null)
            {
                tempUser = new TempUser { Email = email, Otp = otp, CreatedAt = DateTime.UtcNow };
                await _context.TempUsers.AddAsync(tempUser);
            }
            else
            {
                tempUser.Otp = otp;
                tempUser.CreatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            // Send OTP email
            bool emailSent = await _emailService.SendOtpEmailAsync(email, otp);
            if (!emailSent)
            {
                ModelState.AddModelError("", "Failed to send OTP. Try again.");
                return View();
            }

            return RedirectToAction("VerifyOtp", new { email });
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
            if (tempUser == null || tempUser.Otp != otp || tempUser.CreatedAt < DateTime.UtcNow.AddMinutes(-5))
            {
                ModelState.AddModelError("", "Invalid or expired OTP.");
                return View();
            }

            HttpContext.Session.SetString("ResetEmail", email);
            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("ResetEmail")))
            {
                return RedirectToAction("ForgotPassword");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string password)
        {
            string email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View();
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("ResetEmail");
            TempData["SuccessMessage"] = "Password reset successful. You can log in now.";
            return RedirectToAction("Login", "Login");
        }

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
