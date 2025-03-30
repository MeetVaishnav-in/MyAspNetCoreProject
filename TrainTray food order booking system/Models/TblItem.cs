using System;
using System.Collections.Generic;

namespace TrainTray_food_order_booking_system.Models;

public partial class TblItem
{
    public int ItemId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int RestaurantId { get; set; }

    public string? Description { get; set; }

    public string? ItemImage { get; set; }

    public bool ItemStatus { get; set; }

    public int CategoryId { get; set; }

    public virtual TblCategory? Category { get; set; } = null!;

    public virtual ICollection<Order>? Orders { get; set; } = new List<Order>();

    public virtual Restaurant? Restaurant { get; set; } = null!;

    public virtual ICollection<TblCart>? TblCart { get; set; } = new List<TblCart>();
}
