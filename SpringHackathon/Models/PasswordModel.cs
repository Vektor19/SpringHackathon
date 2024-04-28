using System.ComponentModel.DataAnnotations;

namespace SpringHackathon.Models
{
    public class PasswordModel
    {

        public string OldPassword { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "New Password")]
        public string NewPassword { get; set; }
        
        [Required, DataType(DataType.Password), Display(Name = "Current Password")]
        [Compare("NewPassword", ErrorMessage ="New Password and Confirm Password does not match")]
        public string ConfirmPassword { get; set; }
    }
}     