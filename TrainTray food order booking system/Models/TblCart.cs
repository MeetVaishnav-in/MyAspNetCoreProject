using System;
using System.Collections.Generic;

namespace TrainTray_food_order_booking_system.Models;

public partial class TblCart
{
    public int CartId { get; set; }

    public int UserId { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public virtual TblItem Item { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
