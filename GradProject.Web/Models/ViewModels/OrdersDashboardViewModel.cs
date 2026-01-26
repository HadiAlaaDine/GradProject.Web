using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace GradProject.Web.Models.ViewModels
{
    public class OrdersDashboardViewModel
    {
        // عدد الطلبات الكلي
        public int TotalOrders { get; set; }

        // مجموع الإيرادات
        public decimal TotalRevenue { get; set; }

        // آخر 5 طلبات
        public List<OrderRow> RecentOrders { get; set; } = new List<OrderRow>();

        // أكثر 5 منتجات مبيعاً
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