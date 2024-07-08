using System;

namespace FruitStoreModels.Shop
{
    public class AllShopDetails
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageData { get; set; }
        public byte[] ProductImage { get; set; }
        public int Rate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
