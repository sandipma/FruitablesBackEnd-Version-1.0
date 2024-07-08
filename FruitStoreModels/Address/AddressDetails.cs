namespace FruitStoreModels.Address
{
    public class AddressDetails
    {
        public int AddressId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StreetAddress { get; set; }
        public long PostalCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public long PhoneNumber { get; set; }
        public string Email { get; set; }
        public int UserId { get; set; }
        public char IsCurrentSelected { get; set; }
    }
}
