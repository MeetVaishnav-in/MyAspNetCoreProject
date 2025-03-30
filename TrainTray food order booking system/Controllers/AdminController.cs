using Microsoft.AspNetCore.Mvc;
using TrainTray_food_order_booking_system.Models;

namespace TrainTray_food_order_booking_system.Controllers
{
    public class AdminController : Controller
    {
        private readonly TblUserContext _context;

        public AdminController(TblUserContext context)
        {
            _context = context;
        }
        public IActionResult AdminPage()
        {
            return View(_context.Restaurant.ToList());
        }
    }
}
