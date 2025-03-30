using System;
using System.Collections.Generic;

namespace TrainTray_food_order_booking_system.Models;

public partial class TempUser
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Otp { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
