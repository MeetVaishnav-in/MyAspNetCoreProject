using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using TrainTray_food_order_booking_system.Services;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrainTray_food_order_booking_system.Models;

namespace Train.Controllers
{
    public class PaymentController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly TblUserContext _context;

        public PaymentController(HttpClient httpClient, TblUserContext context)
        {
            _httpClient = httpClient;
            _context=context;

        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(decimal amount)
        {
            var keyId = "rzp_test_DTod4Yz00SpEl7";
            var keySecret = "8Ok8BQ7R9UESUZ5heZfzAroK";
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));

            var requestData = new
            {
                amount = amount * 100,  // Convert to paisa
                currency = "INR",
                receipt = "order_rcptid_11",
                payment_capture = 1
            };

            var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            var response = await _httpClient.PostAsync("https://api.razorpay.com/v1/orders", content);
            var responseString = await response.Content.ReadAsStringAsync();

            return Ok(responseString);
        }
        //public IActionResult Confirm(string paymentId)
        //{
        //    if (!string.IsNullOrEmpty(paymentId))
        //    {
        //        // Save payment details to the database
        //        return View("Success");
        //    }
        //    return View("Failure");
        //}

        public IActionResult Confirm(string paymentId)
        {
            if (!string.IsNullOrEmpty(paymentId))
            {
                TempData["PaymentMessage"] = "Payment Successful!";
                return RedirectToAction("Success");
            }
            TempData["PaymentMessage"] = "Payment Failed!";
            return RedirectToAction("Failure");
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrder(int itemId, string seatNo, string coachNo, int quantity, decimal amount, string restaurantName, string station, string platform, string arrivalTime, string customerName, string customerEmail, string customerContact, string paymentId, int trainNo, string trainName)
        {
            //if (trainNo <= 0)
            //{
            //    return BadRequest(new { message = "Invalid train number. Please enter a valid number." });
            //}

            //// Validate Train Name
            //if (string.IsNullOrWhiteSpace(trainName))
            //{
            //    return BadRequest(new { message = "Train name cannot be empty. Please provide a valid train name." });
            //}

            // Validate payment ID
            if (string.IsNullOrEmpty(paymentId))
            {
                return BadRequest(new { message = "Payment failed. Please try again." });
            }

            // Validate required fields
            if (itemId <= 0 || string.IsNullOrWhiteSpace(seatNo) || string.IsNullOrWhiteSpace(coachNo))
            {
                return BadRequest(new { message = "Invalid order details. Please fill all required fields." });
            }

            if (quantity <= 0)
            {
                return BadRequest(new { message = "Quantity must be greater than 0." });
            }

            if (amount <= 0)
            {
                return BadRequest(new { message = "Invalid amount. Please enter a valid price." });
            }

            // Validate user existence
            var user = _context.User.FirstOrDefault(u => u.Email == customerEmail);
            if (user == null)
            {
                return BadRequest(new { message = "Customer email not found. Please provide a registered email." });
            }

            // Validate restaurant existence
            var restaurant = _context.Restaurant.FirstOrDefault(r => r.Name == restaurantName);
            if (restaurant == null)
            {
                return BadRequest(new { message = "Invalid restaurant name. Please select a valid restaurant." });
            }

            // Validate station existence
            var stationEntity = _context.TblStations.FirstOrDefault(s => s.Name == station);
            if (stationEntity == null)
            {
                return BadRequest(new { message = "Invalid station. Please select a valid station." });
            }

            if (arrivalTime == null)
            {
                return BadRequest(new { message = "ArrivalTime should not be null !!." });
            }


            // Parse Platform Number
            int platformNo = 0;
            if (!string.IsNullOrEmpty(platform) && !int.TryParse(platform, out platformNo))
            {
                return BadRequest(new { message = "Invalid platform number. Please enter a valid number." });
            }

            // Create Order Object
            var order = new TrainTray_food_order_booking_system.Models.Order
            {
                UserId = user.Id,
                ItemId = itemId,
                RestaurantId = restaurant.RestaurantId,
                StationId = stationEntity.StationId,
                SeatNo = seatNo,
                CoachNo = coachNo,
                TrainNo = 0,  // Change if you have train number
                TrainName = "", // Change if you have train name
                PlatformNo = platformNo,
                Quantity = quantity,
                OrderDate = DateOnly.FromDateTime(DateTime.Now),
                ArrivalTime = arrivalTime,
                Status = 1, // Assuming 1 = Paid
                Total = amount
            };

            // Save Order to Database
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Order placed successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your order.", error = ex.Message });
            }
        }







    }
}
