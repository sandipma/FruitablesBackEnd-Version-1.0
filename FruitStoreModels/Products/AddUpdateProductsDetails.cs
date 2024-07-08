using Microsoft.AspNetCore.Http;

namespace FruitStoreModels.Products
{
    public class AddUpdateProductsDetails
    {
        public string ProductId { get; set; }
        public string AdminId { get; set; }
        public int CategoryId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public decimal StockQuantity { get; set; }
        public IFormFile ImageData { get; set; }
    }
}
