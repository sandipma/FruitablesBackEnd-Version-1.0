using System;

namespace FruitStoreModels.Auth
{
    public class AccessTokenDetails
    {
        public int AccessTokenId { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessTokenTimePeriod { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }

    }
}
