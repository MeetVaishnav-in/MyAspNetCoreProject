using System;
using System.Collections.Generic;

namespace TrainTray_food_order_booking_system.Models;

public partial class TblCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<TblItem> TblItems { get; set; } = new List<TblItem>();
}
