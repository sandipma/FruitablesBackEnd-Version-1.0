namespace FruitStoreModels.ShopDetails
{
    public class ProductDetail
    {
        public int ProductId { get; set; }
        public int FinalRating { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string ProductDescription { get; set; }
        public int StockQuantity { get; set; }
        public string Image { get; set; }
        public byte[] ProductImage { get; set; }
        public decimal Price { get; set; }
    }
}
