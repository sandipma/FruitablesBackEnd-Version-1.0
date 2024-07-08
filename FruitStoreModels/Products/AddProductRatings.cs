namespace FruitStoreModels.Products
{
    public class AddProductRatings
    {
        public string Review { get; set; }
        public int Rate { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
    }
}
