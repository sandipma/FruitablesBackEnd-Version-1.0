using FruitStoreModels.Category;

namespace FruitStoreRepositories.Interfaces
{
    public interface ICategoryRepository
    {
        public Task<List<CategoryDetails>> GetAllCategoriesAsync();

        public Task<List<CategoryWithProductsCounts>> GetCategoryWithProductsCountsAsync();

        public Task<int> InsertNewCategoryAsync(AddCategoryDetails addCategoryDetails);

        public Task<int> DeleteCategoryByIdAsync(int categoryId);
    }
}
