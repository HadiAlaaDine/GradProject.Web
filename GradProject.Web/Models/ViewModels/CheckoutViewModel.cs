using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GradProject.Web.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, StringLength(200)]
        public string Address1 { get; set; }

        [StringLength(200)]
        public string Address2 { get; set; }

        [Required, StringLength(100)]
        public string City { get; set; }

        [Required, StringLength(100)]
        public string Country { get; set; }

        [StringLength(30)]
        [Phone]
        public string Phone { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
    }
}