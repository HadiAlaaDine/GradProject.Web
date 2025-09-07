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

        // ✅ طريقة الدفع + حالة الطلب
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // ✅ معلومات الشحن / بيانات المستلم
        [Required, StringLength(100)]
        public string ShipFullName { get; set; }

        [Required, StringLength(200)]
        public string ShipAddress1 { get; set; }

        [StringLength(200)]
        public string ShipAddress2 { get; set; }

        [Required, StringLength(100)]
        public string ShipCity { get; set; }

        [Required, StringLength(100)]
        public string ShipCountry { get; set; }

        [StringLength(30)]
        public string ShipPhone { get; set; }

        // Navigation
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}