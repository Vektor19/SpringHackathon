using System.Net.Mail;
using System.Net;
using SpringHackathon.Settings;

namespace SpringHackathon.Services
{
	public class EmailSenderService
	{
		private readonly string _fromMail;
		private readonly string _fromPassword;

		private readonly SmtpClient _smtpClient;

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
