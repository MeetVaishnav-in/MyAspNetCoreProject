using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainTray_food_order_booking_system.Models;

public partial class Restaurant
{
    public int RestaurantId { get; set; }

    [Required(ErrorMessage = "Restaurant Name is required.")]
    [RegularExpression(@"^(?!\d+$)[a-zA-Z0-9\s]+$", ErrorMessage = "Name must contain letters and can include digits but cannot be only digits.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [RegularExpression(@"^(?!\d+$)[a-zA-Z0-9\s,.-]+$", ErrorMessage = "Address must contain letters and can include digits but cannot be only digits.")]
    public string Address { get; set; } = null!;

    public int? StationId { get; set; }

    public string? Logo { get; set; } 

    [Required(ErrorMessage = "PAN number is required.")]
    [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]$", ErrorMessage = "Invalid PAN number. Format: ABCDE1234F")]
    public string Pan { get; set; } = null!;

    [Required(ErrorMessage = "GST number is required.")]
    public string Gst { get; set; } = null!;

    public int Status { get; set; }

    public int UserId { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public virtual TblStation? Station { get; set; }

    public virtual ICollection<TblItem>? TblItems { get; set; } = new List<TblItem>();

    public virtual User? User { get; set; } = null!;

    public virtual ICollection<Order>? Orders { get; set; } = new List<Order>();

    

   
}
