using System;

namespace FruitStoreModels.ApplicationUser
{
    public class UserDetails
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public string Token { get; set; }
        public string UserPasswordHash { get; set; }
        public DateTime TokenExpireTimePeriod { get; set; }
    }
}

