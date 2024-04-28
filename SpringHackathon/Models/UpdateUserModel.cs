using System.ComponentModel.DataAnnotations;

namespace SpringHackathon.Models
{
	public class UpdateUserModel
	{
		[Required]
		public string Username { get; set; }

		[Required, EmailAddress]
		public string NewEmail { get; set; }

		[Required]
		public string OldPassword { get; set; }

		[Required, DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Required, DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
		public string NewPasswordConfirmed { get; set; }
	}
}
