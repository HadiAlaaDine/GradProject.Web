using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradProject.Web.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Display(Name = "Date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "رقم الهاتف ضروري للتواصل")]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "العنوان مطلوب للتوصيل")]
        [Display(Name = "Delivery Address")]
        [DataType(DataType.MultilineText)]
        public string CustomerAddress { get; set; }

        // هون بنخزن شو طلب الزبون كنص (عشان نعرضه بالواتساب بسهولة)
        // مثلاً: "2x Burger, 1x Pepsi"
        public string OrderDetails { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        // حالة الطلب: Pending (قيد الانتظار)
        public string Status { get; set; } = "Pending";

        // نوع الدفع: مثبت على الدفع عند الاستلام
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Cash On Delivery (COD)";

        // عشان نعرف مين اليوزر اللي طلب (اختياري)
        public string UserId { get; set; }
    }
}