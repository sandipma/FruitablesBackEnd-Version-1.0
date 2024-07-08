namespace FruitStoreModels.Cart
{
    public class CartDetails
    {
        public int CartId { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int UserId { get; set; }
        public decimal CurrentQuant { get; set; }
    }
}
