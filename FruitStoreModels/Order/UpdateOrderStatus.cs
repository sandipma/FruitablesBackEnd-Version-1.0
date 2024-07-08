namespace FruitStoreModels.Order
{
    public class UpdateOrderStatus
    {
        public string OrderStatus { get; set; }
        public int UserId { get; set; }
        public int? OrderId { get; set; }
        public string ProductName { get; set; }
        public int? UpdateByAdminId { get; set; }
    }
}
