using System.Net.Mail;
using System.Net;
using SpringHackathon.Settings;

namespace SpringHackathon.Services
{
	/// <summary>
	/// Service for sending emails.
	/// </summary>
	public class EmailSenderService
	{
		private readonly string _fromMail;
		private readonly string _fromPassword;

		private readonly SmtpClient _smtpClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="EmailSenderService"/> class.
		/// </summary>
		/// <param name="emailSenderConfig">The configuration for the email sender.</param>
		public EmailSenderService(EmailSenderConfig emailSenderConfig)
		{
			_fromMail = emailSenderConfig.fromMail;
			_fromPassword = emailSenderConfig.fromPassword;
			_smtpClient = new SmtpClient("smtp.gmail.com")
			{
				Port = 587,
				Credentials = new NetworkCredential(_fromMail, _fromPassword),
				EnableSsl = true
			};
		}

		/// <summary>
		/// Sends an email.
		/// </summary>
		/// <param name="toEmail">The email address to send the email to.</param>
		/// <param name="subject">The subject of the email.</param>
		/// <param name="body">The body of the email.</param>
		/// <param name="isBodyHtml">A value indicating whether the body of the email is HTML.</param>
		public void SendMessage(string toEmail, string subject, string body, bool isBodyHtml = true)
		{
			MailMessage message = new MailMessage();
			message.From = new MailAddress(_fromMail);
			message.Subject = subject;
			message.To.Add(new MailAddress(toEmail));
			message.Body = body;
			message.IsBodyHtml = isBodyHtml;

			_smtpClient.Send(message);
		}
	}
}
