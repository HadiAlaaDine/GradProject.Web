using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GradProject.Web.Models
{
    public class Product
    {
            public int Id { get; set; }

            [Required, StringLength(200)]
            public string Name { get; set; }

            [StringLength(1000)]
            public string Description { get; set; }

            [Range(0, 1_000_000)]
            [DataType(DataType.Currency)]
            public decimal Price { get; set; }

            [Display(Name = "Category")]
            public int CategoryId { get; set; }

            [ForeignKey(nameof(CategoryId))]
            public virtual Category Category { get; set; }

            [ScaffoldColumn(false)]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}