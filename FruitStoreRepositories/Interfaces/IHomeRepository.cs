using FruitStoreModels.Home;

namespace FruitStoreRepositories.Interfaces
{
    public interface IHomeRepository
    {
        public Task<List<TopProductsDetails>> GetTopProductsDetailsAsync(string vegOrFruits);
        public Task<List<BestSellerProducts>> GetBestSellerProductsDetailsAsync();
    }
}
