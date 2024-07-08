namespace FruitStoreModels.Auth
{
    public class ForgotPasswordDetails
    {
        public string UserName { get; set; }
        public string Code { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
    }
}
