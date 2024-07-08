using System;

namespace FruitStoreModels.Order
{
    public class UserOrderDetails
    {
        public int OrderId { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ModeOfPayment { get; set; }
        public string Receipt { get; set; }
        public string OrderStatus { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}
