namespace ExternalService.Interfaces
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string email, string name, string callbackUrl, string subject, string actionName);

        public Task SendOTPByEmailAsync(string email, string name, string subject, int otp);

        public Task SendPromotionalEmailAsync(string email, string name, string subject);
    }
}
