namespace FruitStoreModels.Order
{
    public class CompleteOrderProcess
    {
        public int OrderId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string PaymentId { get; set; }
        public string Signature { get; set; }
        public string ModeOfPayment { get; set; }
        public int UserId { get; set; }
    }
}
