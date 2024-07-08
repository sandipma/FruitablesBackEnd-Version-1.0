using FruitStoreModels.Order;
using FruitStoreModels.Payment;

namespace FruitStoreRepositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<OrderDetails> ProcessOrderAsync(AddOrderDetails orderRequest);
        Task<int> CompleteOrderProcessAsync(CompleteOrderProcess completeOrderProcess);
        Task<int> UpdatePaymentDetailsAsync(UpdatePaymentStatus paymentStatus);
    }
}
