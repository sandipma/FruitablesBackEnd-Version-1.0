namespace FruitStoreModels.Payment
{
    public class UpdatePaymentStatus
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string ModeOfPayment { get; set; }
        public string OrderStatus { get; set; }
    }
}
