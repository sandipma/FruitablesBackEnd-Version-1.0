using FruitStoreModels.Products;

namespace FruitStoreRepositories.Interfaces
{
    public interface IProductsRepository
    {
        public Task<int> InsertUpdateProductsAsync(AddUpdateProductsDetails addUpdateNewProduct);
        public Task<List<ProductsDetails>> GetAllProductsAsync();
        public Task<int> DeleteProductByIdAsync(int productId);

    }
}
