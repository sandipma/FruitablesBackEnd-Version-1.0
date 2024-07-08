namespace FruitStoreModels.Auth
{
    public class ResetPassword
    {
        public string Password { get; set; }
        public string UserId { get; set; }
        public string Code { get; set; }
    }
}
