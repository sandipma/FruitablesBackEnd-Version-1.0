using FruitStoreModels.Admin_Orders;

namespace FruitStoreRepositories.Interfaces
{
    public interface IAdminOrderRepository
    {
        public Task<List<AdminOrder>> GetAllOrdersAsync();
    }
}
