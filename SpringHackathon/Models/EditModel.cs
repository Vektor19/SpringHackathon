using System.ComponentModel.DataAnnotations;

namespace SpringHackathon.Models
{
    public class EditModel
    {

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;
    }
}
