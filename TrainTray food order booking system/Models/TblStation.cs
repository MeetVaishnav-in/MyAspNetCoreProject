using System;
using System.Collections.Generic;

namespace TrainTray_food_order_booking_system.Models;

public partial class TblStation
{
    public int StationId { get; set; }

    public string Name { get; set; } = null!;

    public int StateId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();

    public virtual TblState State { get; set; } = null!;
}
