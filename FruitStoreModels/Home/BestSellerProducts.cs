namespace FruitStoreModels.Home
{
    public class BestSellerProducts
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string Image { get; set; }
        public byte[] ProductImage { get; set; }
        public decimal Price { get; set; }
        public int Rate { get; set; }
    }
}
