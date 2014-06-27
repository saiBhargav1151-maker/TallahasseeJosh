using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace Dqe.Web.Areas.Account.Models
{
    public class ChangePasswordViewModel
    {
        [Display(Name = "Current Password")]
        [Required]
        [StringLength(25)]
        public string CurrentPassword { get; set; }

        [Display(Name = "New Password")]
        [Required]
        [StringLength(25)]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [Required]
        [EqualTo("NewPassword", ErrorMessage = "The password fields do not match.")]
        [StringLength(25)]
        public string ConfirmPassword { get; set; }
    }
}