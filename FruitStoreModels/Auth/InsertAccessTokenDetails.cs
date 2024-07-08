using System;

namespace FruitStoreModels.Auth
{
    public class InsertAccessTokenDetails
    {
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public DateTime? AccessTokenTimePeriod { get; set; }
    }
}
