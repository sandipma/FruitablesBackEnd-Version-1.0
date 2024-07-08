using System;

namespace FruitStoreModels.Payment
{
    public class OrderDetails
    {
        public int OrderId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Charges { get; set; }
        public decimal Total { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }
        public int UserId { get; set; }
        public string RazorPayOrderId { get; set; }
        public string ModeOfPayment { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
