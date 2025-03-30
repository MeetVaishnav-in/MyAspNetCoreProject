using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrainTray_food_order_booking_system.Models;

public class GstController : Controller
{
    private readonly HttpClient _httpClient;

    public GstController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CheckGst(string gstNumber)
    {
        if (string.IsNullOrWhiteSpace(gstNumber))
        {
            return Json(new { success = false, message = "Please enter a GST number." });
        }

        string apiUrl = $"https://appyflow.in/api/verifyGST?key_secret=jf7HieEjSqdhKNS6crdgGEBwqKJ3&gstNo={gstNumber}";

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                GstResponse gstResponse = JsonConvert.DeserializeObject<GstResponse>(jsonResponse);

                if (gstResponse != null && gstResponse.Flag==false)
                {
                    return Json(new { success = true, message = "GST Validation Successful!" });
                }
                else
                {
                    return Json(new { success = false, message = "GST Validation Failed!" });
                }
            }
        }
        catch
        {
            return Json(new { success = false, message = "Error while fetching GST details." });
        }

        return Json(new { success = false, message = "Invalid response from the server." });
    }
}
