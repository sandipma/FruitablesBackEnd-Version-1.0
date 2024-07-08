using FruitStoreModels.Order;
using FruitStoreModels.Payment;

namespace FruitStoreRepositories.Interfaces
{
    public interface IOrderRepository
    {
        public Task<int> InsertOrderDetailsAsync(InsertOrderDetails orderDetails);
        public Task<OrderDetails> GetOrderDetailsByOrderIdAsync(int orderId);
        public Task<List<UserOrderDetails>> GetOrderDetailsByUserIdAsync(int userId);
        public Task<int> UpdateOrderStatusAsync(UpdateOrderStatus updateOrderStatus);
    }
}
