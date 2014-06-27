using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace Dqe.Web.Areas.Account.Models
{
    public class RegisterViewModel
    {
        [Display(Name = "Email")]
        [Required]
        [Email(ErrorMessage = "The Email field is invalid")]
        [StringLength(255)]
        public string UserEmail { get; set; }

        [Required]
        [StringLength(25)]
        public string Password { get; set; }

        [Required]
        [StringLength(25)]
        [EqualTo("Password", ErrorMessage = "The password fields do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(25)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(35)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
    }
}