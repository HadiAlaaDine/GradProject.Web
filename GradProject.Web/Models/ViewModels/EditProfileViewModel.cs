using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GradProject.Web.Models.ViewModels
{
    public class EditProfileViewModel
    {
        [Required, StringLength(256)]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required, EmailAddress, StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name ="Phone Number")]
        public string PhoneNumber { get; set; }
    }
}