using TrainTray_food_order_booking_system.Models;
using Microsoft.AspNetCore.Mvc;


namespace TrainTray_food_order_booking_system.Controllers
{
    public class EncryptionController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new EncryptionModel());
        }

        [HttpPost]
        public IActionResult Encrypt(EncryptionModel model)
        {
            if (!string.IsNullOrEmpty(model.PlainText))
            {
                // Encrypt using BCrypt
                model.EncryptedText = BCrypt.Net.BCrypt.HashPassword(model.PlainText);
            }

            return View("Index", model);
        }

        [HttpPost]
        public IActionResult Verify(EncryptionModel model)
        {
            if (!string.IsNullOrEmpty(model.CheckPlainText) && !string.IsNullOrEmpty(model.CheckEncryptedText))
            {
                // Verify if entered plain text matches the encrypted text
                bool isMatch = BCrypt.Net.BCrypt.Verify(model.CheckPlainText, model.CheckEncryptedText);
                model.MatchResult = isMatch ? "✅ Match: The text is correct." : "❌ No Match: The text does not match.";
            }

            return View("Index", model);
        }
    }
}
