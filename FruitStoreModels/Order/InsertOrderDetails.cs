using System.Data;

namespace FruitStoreModels.Payment
{
    public class InsertOrderDetails
    {
        public DataTable CartDetails { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Charges { get; set; }
        public decimal Total { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }
        public string Currency { get; set; }
        public string Receipt { get; set; }
        public string OrderStatus { get; set; }
        public string CreationDeletion { get; set; }
        public int UserId { get; set; }
        public string RazorPayOrderId { get; set; }
        public string ModeOfPayment { get; set; }
    }
}
