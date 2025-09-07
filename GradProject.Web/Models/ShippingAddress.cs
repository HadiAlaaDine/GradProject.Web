using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GradProject.Web.Models
{
    public class ShippingAddress
    {
        public int Id { get; set; }

        [Required, StringLength(128)]
        public string UserId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, StringLength(20)]
        public string Phone { get; set; }

        [Required, StringLength(200)]
        public string AddressLine1 { get; set; }

        [StringLength(200)]
        public string AddressLine2 { get; set; }

        [Required, StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string State { get; set; }

        [StringLength(20)]
        public string PostalCode { get; set; }

        [Required, StringLength(100)]
        public string Country { get; set; } = "Lebanon";

        public bool IsDefault { get; set; } = false;

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}