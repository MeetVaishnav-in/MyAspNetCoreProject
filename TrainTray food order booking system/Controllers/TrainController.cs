using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TrainTray_food_order_booking_system.Models;

namespace Train.Controllers
{
    public class TrainController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TblUserContext _context;


        public TrainController(IHttpClientFactory httpClientFactory, TblUserContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        public IActionResult AllItems(int stationId)
        {
            var items = _context.TblItems
                .Include(i => i.Restaurant)
                .Where(i => i.Restaurant.StationId == stationId)
                .ToList();

            // Debugging: Check if any items were fetched
            if (items.Count == 0)
            {
                Console.WriteLine("No items found for Station ID: " + stationId);
            }

            ViewBag.StationId = stationId;
            ViewBag.StationName = _context.TblStations
                .Where(s => s.StationId == stationId)
                .Select(s => s.Name)
                .FirstOrDefault();

            ViewBag.Items = items;

            return View();
        }


        public async Task<IActionResult> ItemDetails(int restaurantId)
        {
            var restaurant = await _context.Restaurant
         .Include(r => r.Station) // ✅ Ensure Station is included
         .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);
            if (restaurant == null)
            {
                return NotFound("Restaurant not found.");
            }

            var items = await _context.TblItems
                .Where(i => i.RestaurantId == restaurantId)
                .ToListAsync();

            ViewBag.Restaurant = restaurant;
            ViewBag.Items = items;

            

            ViewBag.Platform = "5";  // Example - Replace with real data
            ViewBag.Timing = "10:30 AM";  // Example - Replace with real data

            Console.WriteLine($"Debug: Platform={ViewBag.Platform}, Timing={ViewBag.Timing}"); // Debugging


            return View();
        }


        [HttpPost]
        public IActionResult AddToCart(int itemId, int quantity, int seatNo, string coachNo)
        {
            //var userId = HttpContext.Session.GetInt32("UserId"); // Assuming user is logged in
            int userId = userId = int.Parse(HttpContext.Session.GetString("UserID")); 

            // Assuming user is logged in
            if (userId == null)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            var item = _context.TblItems.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null)
            {
                return Json(new { success = false, message = "Item not found." });
            }

            var existingCartItem = _context.TblCart.FirstOrDefault(c => c.UserId == userId && c.ItemId == itemId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
            }
            else
            {
                var newCartItem = new TblCart
                {
                    UserId = (int)userId,
                    ItemId = itemId,
                    Quantity = quantity
                };
                _context.TblCart.Add(newCartItem);
            }

            _context.SaveChanges();
            return Json(new { success = true, message = "Item added to cart successfully!" });
        }

        [HttpPost]
        public IActionResult DeleteFromCart(int cartId)
        {
            var cartItem = _context.TblCart.Find(cartId);

            if (cartItem != null)
            {
                _context.TblCart.Remove(cartItem);
                _context.SaveChanges();
            }

            return RedirectToAction("Cart");
        }


        public IActionResult Cart()
        {
            int userId = userId = int.Parse(HttpContext.Session.GetString("UserID"));

            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if not authenticated
            }

            var cartItems = _context.TblCart
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    c.CartId,
                    c.ItemId,
                    c.Quantity,
                    ItemName = c.Item.Name,
                    ItemImage = c.Item.ItemImage,
                    ItemPrice = c.Item.Price,
                    RestaurantName = c.Item.Restaurant.Name,
                    StationName = c.Item.Restaurant.Station.Name
                })
                .ToList();

            return View(cartItems);
        }

        [HttpGet]
        public IActionResult GetCartItems()
        {
            int userId = userId = int.Parse(HttpContext.Session.GetString("UserID"));

            var cartItems = _context.TblCart
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    c.CartId,
                    c.ItemId,
                    c.Quantity,
                    ItemName = c.Item.Name,
                    Station = c.Item.Restaurant.Station.Name
                })
                .ToList();

            return Json(new { items = cartItems });
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            int userId = userId = int.Parse(HttpContext.Session.GetString("UserID"));

            var cartItems = _context.TblCart.Where(c => c.UserId == userId).ToList();

            if (cartItems.Any())
            {
                _context.TblCart.RemoveRange(cartItems);
                _context.SaveChanges();
            }

            return Json(new { success = true, message = "Cart cleared successfully." });
        }



        // ✅ StationDetails Page - Displays details of clicked station
        public async Task<IActionResult> StationDetails(string stationName, string distance, string timing, string delay, string platform, string halt)
        {
            if (string.IsNullOrEmpty(stationName))
            {
                return NotFound("Station details are missing.");
            }

            // ✅ Find StationID from the `TblStation` table
            var station = await _context.TblStations.FirstOrDefaultAsync(s => s.Name == stationName);
            if (station == null)
            {
                return NotFound("Station not found.");
            }

            // ✅ Fetch Restaurants based on StationID
            var restaurants = await _context.Restaurant
                .Where(r => r.StationId == station.StationId)
                .ToListAsync();
            ViewBag.StationId = station.StationId;
            ViewBag.StationName = stationName;
            ViewBag.Distance = distance;
            ViewBag.Timing = timing;
            ViewBag.Delay = delay;
            ViewBag.Platform = platform;
            ViewBag.Halt = halt;
            ViewBag.Restaurants = restaurants; // Send restaurant details to the view
            TempData["Platform"] = ViewBag.Platform;
            TempData["Timing"] = ViewBag.Timing;


            return View();
        }

        // ✅ Fetch Train Status (Use GET instead of POST)
        [HttpGet]
        public async Task<IActionResult> Index(TrainStatusRequest request)
        {
            if (string.IsNullOrEmpty(request.TrainNumber))
            {
                ViewBag.Error = "Please enter a valid train number.";
                return View("Index");
            }

            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"https://rappid.in/apis/train.php?train_no={request.TrainNumber}";

            try
            {
                var response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                // ✅ Fix JSON Parsing (Ensure Model Matches API Response)
                var trainStatus = JsonSerializer.Deserialize<TrainStatusResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (trainStatus != null && trainStatus.Success)
                {
                    ViewBag.TrainStatus = trainStatus;
                }
                else
                {
                    ViewBag.Error = "Failed to retrieve train status.";
                }
            }
            catch (HttpRequestException e)
            {
                ViewBag.Error = $"API Request Failed: {e.Message}";
            }
            catch (JsonException e)
            {
                ViewBag.Error = $"JSON Parsing Error: {e.Message}";
            }

            return View("Index");
        }
    }
}
