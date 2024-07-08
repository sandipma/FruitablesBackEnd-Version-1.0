using System.ComponentModel.DataAnnotations;

namespace FruitStoreModels.ApplicationUser
{
    public class RegisterUser
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserPassword { get; set; }
        public string UserRole { get; set; }
    }
}
