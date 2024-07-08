namespace FruitStoreModels.Auth
{
    public class InsertOTPDetails
    {
        public string UserName { get; set; }
        public int OTP { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public int OTPDuration { get; set; }
    }
}
