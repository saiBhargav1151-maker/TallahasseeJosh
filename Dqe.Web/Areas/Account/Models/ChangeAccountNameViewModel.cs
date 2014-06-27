using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace Dqe.Web.Areas.Account.Models
{
    public class ChangeAccountNameViewModel
    {
        [Display(Name = "Email")]
        [Required]
        [Email(ErrorMessage = "The Email field is invalid")]
        [StringLength(255)]
        public string UserEmail { get; set; }

        [Display(Name = "Confirm Email")]
        [Required]
        [Email(ErrorMessage = "The Confirm Email field is invalid")]
        [EqualTo("UserEmail", ErrorMessage = "The email fields do not match.")]
        [StringLength(255)]
        public string ConfirmUserEmail { get; set; }
    }
}