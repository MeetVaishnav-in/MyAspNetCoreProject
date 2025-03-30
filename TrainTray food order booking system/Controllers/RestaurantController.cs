using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainTray_food_order_booking_system.Models;
using TrainTray_food_order_booking_system.Services;

public class RestaurantController : Controller
{
    private readonly TblUserContext _context;
    private readonly EmailService _emailService;

    public RestaurantController(TblUserContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public IActionResult RestaurantDashboard()
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");
        return View();
    }

    private bool IsRestaurantOwner()
    {
        return HttpContext.Session.GetString("UserType") == "1"; // Assuming 2 = Restaurant Owner
    }

    public IActionResult Menu()
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");

        // Retrieve Restaurant ID from session or context (assuming it's stored during login)
        int restaurantId = GetLoggedInRestaurantId(); // Implement this method based on your session management

        Console.WriteLine(restaurantId);
        // Retrieve menu items for the logged-in restaurant
        var items = _context.TblItems
            .Where(i => i.RestaurantId == restaurantId)
            .Include(i => i.Category)
            .ToList();

        // Retrieve categories for dropdown
        ViewBag.Categories = new SelectList(_context.TblCategories, "CategoryId", "CategoryName");

        return View(items);
    }

    // GET: Create Item
    public IActionResult Create()
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");

        ViewBag.Categories = _context.TblCategories.ToList();
        return View();
    }

    // POST: Create Item
    [HttpPost]
    public async Task<IActionResult> Create(TblItem item, IFormFile ItemImageFile)
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");

        if (ItemImageFile != null && ItemImageFile.Length > 0)
        {
            // Generate unique file name
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ItemImageFile.FileName);
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            // Ensure directory exists
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Full file path
            string filePath = Path.Combine(uploadPath, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ItemImageFile.CopyToAsync(stream);
            }

            // Store relative path in database
            item.ItemImage = "/uploads/" + fileName;
        }


        item.RestaurantId = GetLoggedInRestaurantId();

        if (ModelState.IsValid)
        {
            _context.TblItems.Add(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Menu));
        }

        return View(item);
    }


    public IActionResult Edit(int id)
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");


        var item = _context.TblItems.Find(id);
        if (item == null) return NotFound();
        ViewBag.Categories = _context.TblCategories.ToList();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TblItem item, IFormFile ItemImageFile)
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");

        var existingItem = await _context.TblItems.FindAsync(id);

        if (existingItem == null)
        {
            return NotFound();
        }

        if (ItemImageFile != null && ItemImageFile.Length > 0)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ItemImageFile.FileName);
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ItemImageFile.CopyToAsync(stream);
            }

            existingItem.ItemImage = "/uploads/" + fileName;
        }


        existingItem.Name = item.Name;
        existingItem.Description = item.Description;
        existingItem.CategoryId = item.CategoryId;
        existingItem.Price = item.Price;
        existingItem.ItemStatus = item.ItemStatus;
        existingItem.RestaurantId = GetLoggedInRestaurantId();


        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Menu));
    }


    public IActionResult Delete(int id)
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");

        var item = _context.TblItems.Find(id);
        if (item == null) return NotFound();

        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");

        var item = _context.TblItems.Find(id);
        if (item != null)
        {
            _context.TblItems.Remove(item);
            _context.SaveChanges();
        }

        return RedirectToAction(nameof(Menu));
    }

    private int GetLoggedInRestaurantId()
    {
        var restaurantIdStr = HttpContext.Session.GetString("RestaurantId");
        Console.WriteLine("Session RestaurantId: " + restaurantIdStr);
        int restaurantId = restaurantIdStr != null ? int.Parse(restaurantIdStr) : 0;
        Console.WriteLine("Session Value: " + restaurantIdStr);
        return restaurantId;
    }


    public async Task<IActionResult> Location()
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");

        int restaurantId = GetLoggedInRestaurantId();

        var restaurant = await _context.Restaurant
            .Where(r => r.RestaurantId == restaurantId)
            .Select(r => new
            {
                r.RestaurantId,
                r.Name,
                r.Latitude,
                r.Longitude
            })
            .FirstOrDefaultAsync();

        if (restaurant == null)
        {
            return NotFound("Restaurant not found.");
        }

        return View(restaurant); // Pass a single restaurant to the view
    }


    public IActionResult Orders()
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");

        int restaurantId = GetLoggedInRestaurantId(); // ✅ Get the current restaurant's ID

        var today = DateOnly.FromDateTime(DateTime.Today);

        var orders = _context.Orders
            .Include(o => o.Item)
            .Include(o => o.User)
            .Include(o => o.Station)
            .Where(o => o.OrderDate == today && o.Item.RestaurantId == restaurantId) // ✅ Filter by RestaurantId
            .ToList();

        return View(orders);
    }

    public IActionResult AllOrders()
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");

        int restaurantId = GetLoggedInRestaurantId();

        var orders = _context.Orders
            .Include(o => o.Item)
            .Include(o => o.User)
            .Include(o => o.Station)
            .Where(o => o.Item.RestaurantId == restaurantId) // ✅ Filter by RestaurantId
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View("Orders", orders);
    }

    public IActionResult AllOrdersToday()
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");

        int restaurantId = GetLoggedInRestaurantId();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var orders = _context.Orders
            .Include(o => o.Item)
            .Include(o => o.User)
            .Include(o => o.Station)
            .Where(o => o.OrderDate == today && o.Item.RestaurantId == restaurantId) // ✅ Filter by RestaurantId
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View("Orders", orders);
    }

    public IActionResult OrdersTodayByStatus(int status)
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");

        int restaurantId = GetLoggedInRestaurantId();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var orders = _context.Orders
            .Include(o => o.Item)
            .Include(o => o.User)
            .Include(o => o.Station)
            .Where(o => o.OrderDate == today && o.Status == status && o.Item.RestaurantId == restaurantId) // ✅ Filter by RestaurantId
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View("Orders", orders);
    }

   


    public IActionResult OrdersByStatus(int status)
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");

        int restaurantId = GetLoggedInRestaurantId();

        var orders = _context.Orders
            .Include(o => o.Item)
            .Include(o => o.User)
            .Include(o => o.Station)
            .Where(o => o.Status == status && o.Item.RestaurantId == restaurantId) // ✅ Filter by RestaurantId
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View("Orders", orders);
    }


    [HttpPost]
    public async Task<IActionResult> VerifyOtp(int orderId, string otpInput)
    {
        var orderOtp = await _context.OrderOtps
            .Where(o => o.OrderId == orderId)
            .OrderByDescending(o => o.OtpGeneratedAt) // Get latest OTP
            .FirstOrDefaultAsync();

        if (orderOtp == null || orderOtp.OtpCode != otpInput || DateTime.UtcNow > orderOtp.OtpGeneratedAt.AddMinutes(5))
        {
            return Json(new { success = false, message = "Invalid OTP or expired!" });
        }

        // Mark order as completed
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = 2;
            await _context.SaveChangesAsync();
        }

        // Remove OTP after successful verification
        _context.OrderOtps.Remove(orderOtp);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Order marked as Completed!" });
    }



    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User) // Ensure User data is loaded
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null || order.User == null)
        {
            return NotFound("Order or associated User not found.");
        }

        if (order.Status == 0)
        {
            order.Status = 1; // Move to "Pending Delivery"
            await _context.SaveChangesAsync();
            return RedirectToAction("Orders");
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

            return Json(new { success = true, message = "OTP sent successfully!" });
        }

        return RedirectToAction("Orders");
    }


    [HttpGet]
    public IActionResult SearchOrders()
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");
        return View(); // This will load SearchOrders.cshtml
    }


    [HttpGet]
    public async Task<IActionResult> SearchOrdersJson(string searchQuery)
    {
        Console.WriteLine("🔍 SearchOrdersJson called with query: " + searchQuery);
        int restaurantId = GetLoggedInRestaurantId();
        if (string.IsNullOrEmpty(searchQuery))
        {
            Console.WriteLine("⚠️ Empty search query received.");
            return Json(new List<object>()); // Return empty if no query
        }

        var orders = await _context.Orders
    .Include(o => o.Item)
    .Include(o => o.User)
    .Include(o => o.Station)
    .Where(o => o.Item.RestaurantId == restaurantId &&
    o.User.Name.Contains(searchQuery) ||
                o.TrainName.Contains(searchQuery) ||
                o.Status.ToString().Contains(searchQuery) ||
                o.Station.Name.Contains(searchQuery) ||
                o.SeatNo.ToString().Contains(searchQuery) ||
                o.CoachNo.ToString().Contains(searchQuery) ||
                o.PlatformNo.ToString().Contains(searchQuery) ||
                o.Quantity.ToString().Contains(searchQuery) ||
                o.OrderDate.ToString().Contains(searchQuery) ||
                o.Item.Price.ToString().Contains(searchQuery) ||
                o.Item.Name.ToString().Contains(searchQuery) ||
                o.Total.ToString().Contains(searchQuery))
    .Select(o => new
    {
        o.OrderId,
        UserName = o.User.Name,
        o.TrainName,
        o.SeatNo,
        o.CoachNo,
        o.PlatformNo,
        StationName = o.Station.Name,
        Price = o.Item.Price,
        Item = o.Item.Name,
        o.Quantity,
        OrderDate = o.OrderDate.ToString("yyyy-MM-dd"),
        o.Status,
        o.Total
    })
    .ToListAsync();


        Console.WriteLine($"✅ Found {orders.Count} results.");
        return Json(orders); // Return JSON response
    }





    public IActionResult Reviews()
    {
        if (!IsRestaurantOwner()) return RedirectToAction("AccessDenied", "Home");
        return View();
    }
    public async Task<IActionResult> Register()
    {
        int userId;
        if (!int.TryParse(HttpContext.Session.GetString("UserID"), out userId) || userId == 0)
        {
            return RedirectToAction("Login", "Login"); // Redirect to login if user is not logged in
        }

        ViewBag.stations = _context.TblStations.ToList();

        var existingRestaurant = await _context.Restaurant.FirstOrDefaultAsync(r => r.UserId == userId);
        if (existingRestaurant != null)
        {
            HttpContext.Session.SetString("RestaurantId", existingRestaurant.RestaurantId.ToString());
            return RedirectToAction("RestaurantDashboard");
        }

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Register(Restaurant restaurant, IFormFile? ItemImageFile)
    {
        int userId = int.Parse(HttpContext.Session.GetString("UserID") ?? "0");

        var existingRestaurant = await _context.Restaurant.FirstOrDefaultAsync(r => r.UserId == userId);
        if (existingRestaurant != null)
        {
            HttpContext.Session.SetString("RestaurantId", existingRestaurant.RestaurantId.ToString());
            Console.WriteLine("Session RestaurantId: " + HttpContext.Session.GetString("RestaurantId"));

            return RedirectToAction("RestaurantDashboard");
        }

        if (ItemImageFile != null && ItemImageFile.Length > 0)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ItemImageFile.FileName);
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ItemImageFile.CopyToAsync(stream);
            }

            restaurant.Logo = "/uploads/" + fileName;
        }
        else
        {
            restaurant.Logo = "/uploads/default_logo.png";
        }

        restaurant.UserId = userId;

        _context.Restaurant.Add(restaurant);
        await _context.SaveChangesAsync();

        // ✅ Fetch the saved restaurant's ID
        var savedRestaurant = await _context.Restaurant.FirstOrDefaultAsync(r => r.UserId == userId);
        if (savedRestaurant != null)
        {
            HttpContext.Session.SetString("RestaurantId", savedRestaurant.RestaurantId.ToString()); // ✅ Ensure session is set
        }

        return RedirectToAction("RestaurantDashboard");
    }

    [HttpGet]
    public async Task<IActionResult> GetRestaurants()
    {
        int restaurantId = GetLoggedInRestaurantId();

        // Get restaurant data for the logged-in user
        var existingRestaurant = await _context.Restaurant
            .Where(r => r.RestaurantId == restaurantId) // Use Where() to get a collection
            .Select(r => new
            {
                r.Name,
                r.Latitude,
                r.Longitude
            })
            .FirstOrDefaultAsync(); // Now get a single item

        // Return restaurant details as JSON
        return Json(existingRestaurant);
    }

}