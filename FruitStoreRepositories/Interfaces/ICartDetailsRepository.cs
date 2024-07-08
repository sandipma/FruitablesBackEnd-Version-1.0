using FruitStoreModels.Cart;

namespace FruitStoreRepositories.Interfaces
{
    public interface ICartDetailsRepository
    {
        public Task<int> InsertCartDetailsAsync(AddCartDetails addCartItem);

        public Task<(List<CartDetails>, PriceDetails)> GetProductsCartDetailsByUserIdAsync(int userId);

        public Task<BagCounterDetails> GetBagCounterDetailsByUserIdAsync(int userId);

        public Task<int> DeleteCartItemByIdAsync(int cartId);

        public Task<int> UpdateCartDetailsAsync(UpdateCartDetails updateCartItem);

        public Task SetSchedularAsync();
    }
}
