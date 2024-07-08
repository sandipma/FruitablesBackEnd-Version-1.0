namespace FruitStoreModels.Cart
{
    public class UpdateCartDetails
    {
        public int CartId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public int UserId { get; set; }
        public string ProductName { get; set; }
        public string IncreaseOrDecrease { get; set; }
        public decimal TotalQuant { get; set; }
    }
}
