using System;
using Web.Models.Data;

namespace Web.Models.ViewModels.Store
{
    public class OrderViewModel
    {
        public OrderViewModel() {}

        public OrderViewModel(OrderDTO row)
        {
            OrderId = row.OrderId;
            UserId = row.UserId;
            CreatedAt = row.CreatedAt;
        }

        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}