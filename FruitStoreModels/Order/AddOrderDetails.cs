using FruitStoreModels.Order;
using System.Collections.Generic;

namespace FruitStoreModels.Payment
{
    public class AddOrderDetails
    {
        public List<OrderCartDetails> cartDetails { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Charges { get; set; }
        public decimal Total { get; set; }
        public int UserId { get; set; }
        public string ModeOfPayment { get; set; }
    }
}
