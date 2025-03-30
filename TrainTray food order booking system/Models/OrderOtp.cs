using System;
using System.Collections.Generic;

namespace TrainTray_food_order_booking_system.Models;

public partial class OrderOtp
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string OtpCode { get; set; } = null!;

    public DateTime OtpGeneratedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
