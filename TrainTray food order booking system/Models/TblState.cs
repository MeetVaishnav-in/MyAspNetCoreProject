using System;
using System.Collections.Generic;

namespace TrainTray_food_order_booking_system.Models;

public partial class TblState
{
    public int StateId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<TblStation> TblStations { get; set; } = new List<TblStation>();
}
