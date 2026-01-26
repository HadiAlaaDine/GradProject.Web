using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GradProject.Web.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int OrdersCount { get; set; }
        public DateTime? LastOrderAt { get; set; }
    }
}