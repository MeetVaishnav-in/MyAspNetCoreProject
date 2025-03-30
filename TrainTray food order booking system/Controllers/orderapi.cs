using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TrainTray_food_order_booking_system.Models;
using TrainTray_food_order_booking_system.Services;

namespace TrainTray_food_order_booking_system.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TblUserContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(TblUserContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            // Check if user is blocked
            var failedAttempt = await _context.FailedLoginAttempts.FirstOrDefaultAsync(f => f.Email == model.Email);
            if (failedAttempt != null && failedAttempt.BlockedUntil.HasValue && failedAttempt.BlockedUntil > DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Account locked. Try again later." });
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                if (failedAttempt == null)
                {
                    failedAttempt = new FailedLoginAttempt { Email = model.Email, AttemptCount = 1 };
                    await _context.FailedLoginAttempts.AddAsync(failedAttempt);
                }
                else
                {
                    failedAttempt.AttemptCount++;
                    if (failedAttempt.AttemptCount >= 3)
                    {
                        failedAttempt.BlockedUntil = DateTime.UtcNow.AddMinutes(5);
                        return Unauthorized(new { message = "Too many failed attempts. Try again in 5 minutes." });
                    }
                }

                await _context.SaveChangesAsync();
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Reset failed attempts on successful login
            if (failedAttempt != null)
            {
                _context.FailedLoginAttempts.Remove(failedAttempt);
                await _context.SaveChangesAsync();
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful",
                token,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Type
                }
            });
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("UserType", user.Type.ToString())
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class VerifyOtpRequest
    {
        public int OrderId { get; set; }
        public string OtpInput { get; set; }
    }

    [Route("api/orders")]
    [ApiController]
    public class OrderApiController : ControllerBase
    {
        private readonly TblUserContext _context;
        private readonly EmailService _emailService;

        public OrderApiController(TblUserContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var orderOtp = await _context.OrderOtps
                .Where(o => o.OrderId == request.OrderId)
                .OrderByDescending(o => o.OtpGeneratedAt) // Get latest OTP
                .FirstOrDefaultAsync();

            if (orderOtp == null || orderOtp.OtpCode != request.OtpInput || DateTime.UtcNow > orderOtp.OtpGeneratedAt.AddMinutes(5))
            {
                return BadRequest(new { success = false, message = "Invalid OTP or expired!" });
            }

            // Mark order as completed
            var order = await _context.Orders.FindAsync(request.OrderId);
            if (order != null)
            {
                order.Status = 2; // Set status to "Completed"
                await _context.SaveChangesAsync();
            }

            // Remove OTP after successful verification
            _context.OrderOtps.Remove(orderOtp);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Order marked as Completed!" });
        }

        [HttpPost("update-status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User) // Ensure User data is loaded
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null || order.User == null)
            {
                return NotFound(new { success = false, message = "Order or associated User not found." });
            }

            if (order.Status == 0)
            {
                order.Status = 1; // Move to "Pending Delivery"
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status updated to Pending Delivery!" });
            }

            if (order.Status == 1) // Moving to "Completed" requires OTP
            {
                // Generate OTP
                var otp = new Random().Next(100000, 999999).ToString();
                var orderOtp = new OrderOtp
                {
                    OrderId = order.OrderId,
                    OtpCode = otp,
                    OtpGeneratedAt = DateTime.UtcNow
                };

                _context.OrderOtps.Add(orderOtp);
                await _context.SaveChangesAsync();

                // Send OTP via Email
                await _emailService.SendOtpEmailAsync(order.User.Email, otp);

                return Ok(new { success = true, message = "OTP sent successfully!" });
            }

            return BadRequest(new { success = false, message = "Invalid order status transition!" });
        }


        [HttpGet("today/status/{Res}/{status}")]
        public async Task<IActionResult> GetOrdersTodayByStatus(int status, int Res)
        {
            int restaurantId = Res;
            var today = DateOnly.FromDateTime(DateTime.Today);

            var orders = await _context.Orders
                .Include(o => o.Item)
                .Include(o => o.User)
                .Include(o => o.Station)
                .Where(o => o.OrderDate == today && o.Status == status && o.Item.RestaurantId == restaurantId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.OrderId,
                    UserName = o.User.Name,
                    Contact=o.User.Mobile,
                    ItemName=o.Item.Name,
                    o.TrainName,
                    o.SeatNo,
                    o.CoachNo,
                    o.PlatformNo,
                    ArrivalTime = o.ArrivalTime,
                    StationName = o.Station.Name,
                    Price = o.Item.Price,
                    o.Quantity,
                    o.Total
                })
                .ToListAsync();

            return Ok(orders);
        }



    }
}
