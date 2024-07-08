using FruitStoreModels.About_Us;

namespace FruitStoreRepositories.Interfaces
{
    public interface IAboutUsRepository
    {
        public Task<OverAllReview> GetOverAllReviewAsync();
    }
}
