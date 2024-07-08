namespace FruitStoreModels.Auth
{
    public class OTPDetails
    {
        public int UserId { get; set; }
        public int OTP { get; set; }
        public int OTPId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
