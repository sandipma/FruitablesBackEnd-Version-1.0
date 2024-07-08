using FruitStoreModels.Products;
using FruitStoreModels.ShopDetails;

namespace FruitStoreRepositories.Interfaces
{
    public interface IShopDetailsRepository
    {
        public Task<(List<RatingDetails>, ProductDetail)> GetProductAndRatingDetailsAsync(int productId);
        public Task<int> PostProductCommentAsync(AddProductRatings rating);
        public Task<int> CheckProductPurchasingAsync(int UserId, int productId);
    }
}
