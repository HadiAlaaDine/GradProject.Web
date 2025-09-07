using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GradProject.Web.Models
{
    public enum PaymentMethod
    {
        CashOnDelivery = 0,
        Online = 1
    }

    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Shipped = 2,
        Completed = 3,
        Cancelled = 4
    }

    public class Order
    {
        public int Id { get; set; }

        [Required, StringLength(128)]
        public string UserId { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Currency)]
        public decimal Total { get; set; }

        // ✅ طريقة الدفع
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        // ✅ حالة الطلب
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Navigation
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}