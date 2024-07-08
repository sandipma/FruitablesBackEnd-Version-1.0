namespace FruitStoreModels.Products
{
    public class ProductsDetails
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public decimal StockQuantity { get; set; }
        public string ImageData { get; set; }
        public string ImageName { get; set; }
        public byte[] ProductImage { get; set; }
    }
}
