using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace Dqe.Web.Areas.Account.Models
{
    public class ResetViewModel
    {
        [Display(Name = "Email")]
        [Required]
        [Email(ErrorMessage = "The Email field is invalid")]
        [StringLength(255)]
        public string UserEmail { get; set; }
    }
}