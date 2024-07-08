using System;

namespace FruitStoreModels.ShopDetails
{
    public class RatingDetails
    {
        public int RatingId { get; set; }
        public string Review { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Rate { get; set; }
    }
}
