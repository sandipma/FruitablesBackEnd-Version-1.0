using System;

namespace FruitStoreModels.Admin_Orders
{
    public class AdminOrder
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StreetAddress { get; set; }
        public long PostalCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public long PhoneNumber { get; set; }
        public string AddressEmail { get; set; }
        public int OrderId { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Charges { get; set; }
        public decimal Total { get; set; }
        public string Receipt { get; set; }
        public string OrderDetails { get; set; }
        public string RazorPayOrderId { get; set; }
        public string ModeOfPayment { get; set; }
        public int OrderStatusId { get; set; }
        public string OrderStatusDetails { get; set; }
        public string UpdateByAdminId { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}
