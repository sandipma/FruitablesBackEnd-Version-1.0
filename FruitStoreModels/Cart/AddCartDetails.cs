using Microsoft.AspNetCore.Http;

namespace FruitStoreModels.Cart
{
    public class AddCartDetails
    {
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
        public int UserId { get; set; }
        public decimal CurrentQuant { get; set; }
    }
}
