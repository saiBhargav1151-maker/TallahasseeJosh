using System.ComponentModel.DataAnnotations;

namespace Dqe.Web.Areas.Account.Models
{
    public class UserProfileViewModel
    {
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