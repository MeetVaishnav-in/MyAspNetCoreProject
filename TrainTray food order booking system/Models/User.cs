using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TrainTray_food_order_booking_system.Models;

public partial class User
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    [RegularExpression("^(?![2345])[0-9]{10}$", ErrorMessage = "Mobile number should not start with 2, 3, or 4 and must be 10 digits.")]
    public string Mobile { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [CustomEmailValidation]
    public string Email { get; set; } = null!;


    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$",
    ErrorMessage = "Password must be at least 8 characters long, contain at least one special character, one uppercase letter, and one number.")]
    public string Password { get; set; } = null!;


    public int Type { get; set; }

    public virtual ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
    public virtual ICollection<Order>? Orders { get; set; } = new List<Order>();
    public virtual ICollection<TblCart> TblCart { get; set; } = new List<TblCart>();

}