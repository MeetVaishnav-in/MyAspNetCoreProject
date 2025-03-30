using System;
using System.Collections.Generic;

namespace TrainTray_food_order_booking_system.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public int ItemId { get; set; }

    public int RestaurantId { get; set; }

    public int StationId { get; set; }

    public string SeatNo { get; set; } = null!;

    public string CoachNo { get; set; } = null!;

    public int TrainNo { get; set; }

    public string TrainName { get; set; } = null!;

    public int PlatformNo { get; set; }

    public int Quantity { get; set; }

    public DateOnly OrderDate { get; set; }

    public string ArrivalTime { get; set; } = null!;

    public byte Status { get; set; }

    public decimal? Total { get; set; }

    public virtual TblItem Item { get; set; } = null!;

    public virtual ICollection<OrderOtp> OrderOtps { get; set; } = new List<OrderOtp>();

    public virtual Restaurant Restaurant { get; set; } = null!;

    public virtual TblStation Station { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
