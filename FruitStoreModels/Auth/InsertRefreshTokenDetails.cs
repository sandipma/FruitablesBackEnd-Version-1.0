using System;

namespace FruitStoreModels.Auth
{
    public class InsertRefreshTokenDetails
    {
        public string Email { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenTimePeriod { get; set; }
    }
}
