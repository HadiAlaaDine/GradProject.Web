using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GradProject.Web.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required, StringLength(128)]
        public string UserId { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Currency)]
        public decimal Total { get; set; }

        // Navigation
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}