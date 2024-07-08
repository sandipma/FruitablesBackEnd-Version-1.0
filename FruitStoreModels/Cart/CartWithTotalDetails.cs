using System.Collections.Generic;

namespace FruitStoreModels.Cart
{
    public class CartWithTotalDetails
    {
        public List<CartDetails> CartDetails { get; set; }
        public PriceDetails PriceDetails { get; set; }
    }
}
