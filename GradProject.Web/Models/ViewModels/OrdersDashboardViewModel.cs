using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace GradProject.Web.Models.ViewModels
{
    public class OrdersDashboardViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<OrderRow> RecentOrders { get; set; } = new List<OrderRow>();
        public List<TopProductRow> TopProducts { get; set; } = new List<TopProductRow>();
    }

    public class OrderRow
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Total { get; set; }
    }

    public class TopProductRow
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}