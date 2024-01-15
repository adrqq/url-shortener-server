using System.Net;
using System.Net.Mail;
using UrlShorty.Exceptions;
using static System.Net.WebRequestMethods;

namespace UrlShorty.Services
{
    public class MailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly IConfiguration _configuration;
        private readonly string _smtpHost;
        private readonly string _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;

            _smtpHost = _configuration["AppSettings:SmtpHost"] ?? throw new ApiError(400, "Smtp host not specified");
            _smtpPort = _configuration["AppSettings:SmtpPort"] ?? throw new ApiError(400, "Smtp port not specified");
            _smtpUser = _configuration["AppSettings:SmtpUser"] ?? throw new ApiError(400, "Smtp user not specified");
            _smtpPassword = _configuration["AppSettings:SmtpPassword"] ?? throw new ApiError(400, "Smtp password not specified");

            _smtpClient = new SmtpClient
            {
                Host = _smtpHost,
                Port = Convert.ToInt32(_smtpPort),
                Credentials = new NetworkCredential(_smtpUser, _smtpPassword),
                EnableSsl = true,
            };
        }

        public async Task SendActivationMail(string to, string link)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUser),
                Subject = "Account activation on UrlShorty",
                Body = $"<div><h1>For activation follow the link</h1><a href='{"http://localhost:5075/api/account/activate/" + link}'>{"http://localhost:5075/api/account/activate/" + link}</a></div>",
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);

            await _smtpClient.SendMailAsync(mailMessage);
        }
    }
}
