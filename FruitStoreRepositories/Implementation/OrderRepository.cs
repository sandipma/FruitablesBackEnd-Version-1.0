using Dapper;
using FruitStoreModels.Payment;
using FruitStoreRepositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Data;
using FruitStoreModels.Order;

namespace FruitStoreRepositories.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderRepository> _logger;
        public OrderRepository(IConfiguration configuration, ILogger<OrderRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        //... Method for get order details by order Id...//
        public async Task<OrderDetails> GetOrderDetailsByOrderIdAsync(int orderId)
        {
            try
            {
                _logger.LogInformation($"Fetching records for order details by order Id starts in method GetOrderDetailsByOrderIdAsync Id : {orderId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {

                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();

                    parameters.Add("@OrderId", orderId);

                    _logger.LogInformation("Executing stored procedure stp_GetOrderDetailsByOrderId \n");

                    var orderDetails = await dbConnection.QueryFirstOrDefaultAsync<OrderDetails>(
                        "stp_GetOrderDetailsByOrderId",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (orderDetails != null)
                    {
                        _logger.LogInformation($"Fetching records for order details by order Id from DB succeeded in method GetOrderDetailsByOrderIdAsync Id : {orderId} \n");

                    }
                    else
                    {
                        _logger.LogError($"Fetching records for order details by order Id from DB not found in method GetOrderDetailsByOrderIdAsync Id : {orderId} \n");
                    }
                    return orderDetails;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching records for order details by order Id in method GetOrderDetailsByOrderIdAsync Id : {orderId} : {ex.Message} \n");
                throw;
            }
        }

        //... Method for get order details by user Id...//
        public async Task<List<UserOrderDetails>> GetOrderDetailsByUserIdAsync(int userId)
        {
            try
            {
                List<UserOrderDetails> userOrderDetails = null;

                _logger.LogInformation($"Fetching records for order details by user Id starts in method GetOrderDetailsByUserIdAsync Id : {userId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {

                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();

                    parameters.Add("@UserId", userId);

                    _logger.LogInformation("Executing stored procedure stp_GetOrderDetailsByUserId \n");

                    var orderDetails = await dbConnection.QueryAsync<UserOrderDetails>(
                        "stp_GetOrderDetailsByUserId",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (orderDetails != null)
                    {
                        userOrderDetails = orderDetails.ToList();
                        _logger.LogInformation($"Fetching records for order details by user Id from DB succeeded in method GetOrderDetailsByUserIdAsync Id : {userId} \n");

                    }
                    else
                    {
                        _logger.LogError($"Fetching records for order details by user Id from DB not found in method GetOrderDetailsByUserIdAsync Id : {userId} \n");
                    }
                    return userOrderDetails;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching records for order details by user Id in method GetOrderDetailsByUserIdAsync Id : {userId} : {ex.Message} \n");
                throw;
            }
        }

        //... Method for insert order details...//
        public async Task<int> InsertOrderDetailsAsync(InsertOrderDetails orderDetails)
        {
            try
            {
                _logger.LogInformation($"Inserting order details for products starts in method InsertOrderDetailsAsync for userId : {orderDetails.UserId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@CartDetails", orderDetails.CartDetails.AsTableValuedParameter("OrderCartDetails"));
                    parameters.Add("@Subtotal", orderDetails.SubTotal);
                    parameters.Add("@Charges", orderDetails.Charges);
                    parameters.Add("@Total", orderDetails.Total);
                    parameters.Add("@AmountPaid", orderDetails.AmountPaid);
                    parameters.Add("@AmountDue", orderDetails.AmountDue);
                    parameters.Add("@Currency", orderDetails.Currency);
                    parameters.Add("@Receipt", orderDetails.Receipt);
                    parameters.Add("@OrderStatus", orderDetails.OrderStatus);
                    parameters.Add("@UserId", orderDetails.UserId);
                    parameters.Add("@RazorPayOrderId", orderDetails.RazorPayOrderId);
                    parameters.Add("@ModeOfPayment", orderDetails.ModeOfPayment);
                    parameters.Add("@InsertedOrderId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    _logger.LogInformation("Executing stored procedure stp_SetOrderDetails...");

                    await dbConnection.ExecuteScalarAsync<int>(
                        "stp_SetOrderDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    // Retrieve the output parameter value
                    object insertedOrderIdObj = parameters.Get<object>("@InsertedOrderId");

                    // Check for DBNull and convert to the appropriate type
                    int cartId = insertedOrderIdObj != DBNull.Value ? Convert.ToInt32(insertedOrderIdObj) : 0;

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (cartId != 0)
                    {
                        _logger.LogInformation($"Order details add successfully from DB in method InsertOrderDetailsAsync for userId : {orderDetails.UserId} \n");
                    }
                    else
                    {
                        _logger.LogError($"Order details inserting data to the DB failed in method InsertOrderDetailsAsync for userId : {orderDetails.UserId} \n");
                    }
                    return cartId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while inserting order details data in method InsertOrderDetailsAsync for userId : {orderDetails.UserId} : {ex.Message} \n");
                throw;
            }
        }

        //... Method for update order status...//
        public async Task<int> UpdateOrderStatusAsync(UpdateOrderStatus updateOrderStatus)
        {
            try
            {
                _logger.LogInformation($"Updating order status starts in method UpdateOrderStatusAsync Id : {updateOrderStatus.UserId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {

                    _logger.LogInformation("Attempting to open database connection \n");

                    int updatedStatusId = 0;

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@OrderStatus", updateOrderStatus.OrderStatus);
                    parameters.Add("@UserId", updateOrderStatus.UserId);
                    parameters.Add("@OrderId", updateOrderStatus.OrderId);
                    parameters.Add("@ProductName", updateOrderStatus.ProductName);
                    parameters.Add("@UpdateByAdminId", updateOrderStatus.UpdateByAdminId);
                    parameters.Add("@ReturnStatusId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    _logger.LogInformation("Executing stored procedure stp_UpdateOrderStatusDetails \n");

                    await dbConnection.ExecuteScalarAsync<int>(
                       "stp_UpdateOrderStatusDetails",
                       parameters,
                       commandType: CommandType.StoredProcedure
                   );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    updatedStatusId = parameters.Get<int>("@ReturnStatusId");

                    if (updatedStatusId != 0)
                    {
                        _logger.LogInformation($"Updating order status from DB succeeded in method UpdateOrderStatusAsync Id : {updateOrderStatus.UserId} \n");

                    }
                    else
                    {
                        _logger.LogError($"Updating order status from DB failed in method UpdateOrderStatusAsync Id : {updateOrderStatus.UserId} \n");
                    }

                    return updatedStatusId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating order status in method UpdateOrderStatusAsync Id : {updateOrderStatus.UserId} : {ex.Message} \n");
                throw;
            }
        }
    }
}
