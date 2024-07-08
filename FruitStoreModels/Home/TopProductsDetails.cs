namespace FruitStoreModels.Home
{
    public class TopProductsDetails
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string ProductDescription { get; set; }
        public string Image { get; set; }
        public byte[] ProductImage { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
