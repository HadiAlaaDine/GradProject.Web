using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GradProject.Web.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        // FK: Order
        [Required]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public virtual Order Order { get; set; }

        // FK: Product (خليه non-cascade حتى لو انمسح المنتج تبقى السجلات)
        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; }

        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }   // snapshot

        [NotMapped]
        public decimal Subtotal => UnitPrice * Quantity;
    }
}