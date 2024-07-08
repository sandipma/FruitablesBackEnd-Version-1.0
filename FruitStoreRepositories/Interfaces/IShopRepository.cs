using FruitStoreModels.Shop;

namespace FruitStoreRepositories.Interfaces
{
    public interface IShopRepository
    {
        public Task<List<AllShopDetails>> GetAllShopDetailsAsync(ShopFilterCriterio filterCriterio);
    }
}
