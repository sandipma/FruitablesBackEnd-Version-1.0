using System;

namespace FruitStoreModels.Auth
{
    public class RefreshTokenDetails
    {
        public int RefreshTokenId { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenTimePeriod { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
    }
}
