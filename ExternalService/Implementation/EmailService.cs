using ExternalService.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using static System.Net.WebRequestMethods;

namespace ExternalService.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        //...Method for send email to user for reset password...//
        public async Task SendEmailAsync(string email, string name, string callbackUrl, string subject, string actionName)
        {
            try
            {
                _logger.LogInformation($"Attempting to send email in method SendEmail for email : {email} \n");

                string body = string.Empty;

                if (actionName == "ForgotPassword")
                    body = "<p>Hi {0},</p><p>Welcome to the fruitables password reset process..</p><p>Please click on the link below to reset your login password.</p><p>{1}</p><p>Thanks,<br>Team Fruitables</p>";

                var message = new MailMessage();

                message.To.Add(new MailAddress(email));

                message.From = new MailAddress(_configuration["SMTPDetails:MailFrom"],"Fruitables");

                message.Subject = subject;

                if (actionName == "ForgotPassword")

                message.Body = string.Format(body, name, callbackUrl);

                message.IsBodyHtml = true;
                int port = _configuration.GetValue<int>("SMTPDetails:Port");

                _logger.LogInformation($"SMTP client setup started for email : {email} \n");

                using (var smtp = new SmtpClient(_configuration["SMTPDetails:SMTPMail"], port))
                {
                    _logger.LogInformation($"SMTP client created successfully : {email} \n");

                    smtp.Credentials = new NetworkCredential(_configuration["SMTPDetails:MailFrom"], _configuration["SMTPDetails:SMTPAppPassword"]);
                    smtp.EnableSsl = true;

                    _logger.LogInformation($"SMTP credentials configured for email : {email} \n");

                    await smtp.SendMailAsync(message);

                    _logger.LogInformation($"Email sent successfully in method SendEmail for email : {email} \n");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while sending email in method SendEmail : {ex.Message} \n");
            }

        }

        //...Method for send OTP to user...//
        public async Task SendOTPByEmailAsync(string email, string name, string subject, int otp)
        {
            try
            {
                _logger.LogInformation($"Attempting to send OTP in method SendOTPByEmail for email : {email} \n");

                string body = string.Empty;

                body = $"<p>Hi...{name}</p><p>Your OTP for login is : {otp}</p><p>Thanks,<br>Team Fruitables</p>";

                var message = new MailMessage();

                message.To.Add(new MailAddress(email));

                message.From = new MailAddress(_configuration["SMTPDetails:MailFrom"],"Fruitables");

                message.Subject = subject;

                message.Body = string.Format(body, name);

                message.IsBodyHtml = true;
                int port = _configuration.GetValue<int>("SMTPDetails:Port");

                _logger.LogInformation($"SMTP client setup started for email : {email} \n");

                using (var smtp = new SmtpClient(_configuration["SMTPDetails:SMTPMail"], port))
                {
                    _logger.LogInformation($"SMTP client created successfully : {email} \n");

                    smtp.Credentials = new NetworkCredential(_configuration["SMTPDetails:MailFrom"], _configuration["SMTPDetails:SMTPAppPassword"]);
                    smtp.EnableSsl = true;

                    _logger.LogInformation($"SMTP credentials configured for email : {email} \n");

                    await smtp.SendMailAsync(message);

                    _logger.LogInformation($"Email sent successfully OTP in method SendOTPByEmail for email : {email} \n");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while sending OTP in method SendOTPByEmail for email : {email} : {ex.Message} \n");
            }
        }

        //...Method for send promotional message to user after registered...//
        public async  Task SendPromotionalEmailAsync(string email, string name, string subject)
        {
            try
            {
                _logger.LogInformation($"Attempting to send promotional email in method SendPromotionalEmail to email : {email} \n");
                string body = string.Empty;

                body = $"<p>Hi...{name}</p>Thank you for join with us.Today the whole world is looking for vendors who can deliver locally produced, organic fruits & vegetables.<br> But," +
                    $" you despite having a wide range of natural & seasonal fruits are not able to beat your competitors & acquire more customers.<br> \r\n\r\n" +
                    $"This may be because your possible customers may not be well-informed of the high-quality fruits & dedicated services you provide.<br> " +
                    $"Start promoting the fruits & vegetable products via online media & land your services straight onto the fingertips of your customers.<p></p>" +
                    $"<p>Team Fruitables</p>";

                var message = new MailMessage();

                message.To.Add(new MailAddress(email));

                message.From = new MailAddress(_configuration["SMTPDetails:MailFrom"],"Fruitables");

                message.Subject = subject;

                message.Body = string.Format(body, name);

                message.IsBodyHtml = true;
                int port = _configuration.GetValue<int>("SMTPDetails:Port");

                _logger.LogInformation($"SMTP client setup started for email : {email} \n");

                using (var smtp = new SmtpClient(_configuration["SMTPDetails:SMTPMail"], port))
                {
                    _logger.LogInformation($"SMTP client created successfully : {email} \n");

                    smtp.Credentials = new NetworkCredential(_configuration["SMTPDetails:MailFrom"], _configuration["SMTPDetails:SMTPAppPassword"]);
                    smtp.EnableSsl = true;

                    _logger.LogInformation($"SMTP credentials configured for email : {email} \n");

                    await smtp.SendMailAsync(message);

                    _logger.LogInformation($"Successfully sent promotional email to user in method SendPromotionalEmail for email : {email} \n");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while sent promotional email to user in method SendPromotionalEmail for email : {email} : {ex.Message} \n");
            }
        }

        //...Method for send promotional message to user after registered...//
        public async Task SendGetInTouchEmailAsync(string email, string name, string subject)
        {
            try
            {
                _logger.LogInformation($"Attempting to send get in touch email in method SendGetInTouchEmailAsync to email : {email} \n");
                string body = string.Empty;

                body = $"<p>Hi...{name}</p>Thank you for join with us.Today the whole world is looking for vendors who can deliver locally produced, organic fruits & vegetables.<br> But," +
                    $" you despite having a wide range of natural & seasonal fruits are not able to beat your competitors & acquire more customers.<br> \r\n\r\n" +
                    $"This may be because your possible customers may not be well-informed of the high-quality fruits & dedicated services you provide.<br> " +
                    $"Start promoting the fruits & vegetable products via online media & land your services straight onto the fingertips of your customers.<p></p>" +
                    $"<p>Team Fruitables</p>";

                var message = new MailMessage();

                message.To.Add(new MailAddress(email));

                message.From = new MailAddress(_configuration["SMTPDetails:MailFrom"], "Fruitables");

                message.Subject = subject;

                message.Body = string.Format(body, name);

                message.IsBodyHtml = true;
                int port = _configuration.GetValue<int>("SMTPDetails:Port");

                _logger.LogInformation($"SMTP client setup started for email : {email} \n");

                using (var smtp = new SmtpClient(_configuration["SMTPDetails:SMTPMail"], port))
                {
                    _logger.LogInformation($"SMTP client created successfully : {email} \n");

                    smtp.Credentials = new NetworkCredential(_configuration["SMTPDetails:MailFrom"], _configuration["SMTPDetails:SMTPAppPassword"]);
                    smtp.EnableSsl = true;

                    _logger.LogInformation($"SMTP credentials configured for email : {email} \n");

                    await smtp.SendMailAsync(message);

                    _logger.LogInformation($"Successfully sent promotional email to user in method SendPromotionalEmail for email : {email} \n");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while sent promotional email to user in method SendPromotionalEmail for email : {email} : {ex.Message} \n");
            }
        }
    }
}
