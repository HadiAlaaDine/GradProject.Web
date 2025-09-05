using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GradProject.Web.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        // رابط المستخدم (ASP.NET Identity)
        [Required]
        [StringLength(128)]
        public string UserId { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;
    }
}