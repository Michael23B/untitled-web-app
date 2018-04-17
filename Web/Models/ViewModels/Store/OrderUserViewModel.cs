using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Web.Models.ViewModels.Store
{
    public class OrderUserViewModel
    {
        [DisplayName("Order Id")]
        public int OrderId { get; set; }
        [DisplayName("Username")]
        public string UserName { get; set; }
        public decimal Total { get; set; }
        [DisplayName("Order Date")]
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, int> ProductsAndQty { get; set; }
    }
}