using System;
using System.Collections.Generic;

namespace TrainTray_food_order_booking_system.Models;

public partial class FailedLoginAttempt
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public int AttemptCount { get; set; }

    public DateTime? BlockedUntil { get; set; }
}
