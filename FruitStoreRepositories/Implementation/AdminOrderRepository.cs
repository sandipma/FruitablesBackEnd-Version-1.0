using FruitStoreModels.Admin_Orders;
using FruitStoreModels.Products;
using FruitStoreRepositories.Interfaces;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Dapper;

namespace FruitStoreRepositories.Implementation
{
    public class AdminOrderRepository : IAdminOrderRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminOrderRepository> _logger;
        public AdminOrderRepository(ILogger<AdminOrderRepository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        //... Method for get all orders details for admin...//
        public async Task<List<AdminOrder>> GetAllOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching details for orders starts in method GetAllOrdersAsync \n");

                List<AdminOrder> orderLists = null;

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    _logger.LogInformation("Executing stored procedure stp_GetAllProducts \n");

                    var orderDetails = await dbConnection.QueryAsync<AdminOrder>(
                        "stp_GetAllOrders",
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (orderDetails.Any())
                    {
                        _logger.LogInformation("Fetching details for orders from DB succeeded in method GetAllOrdersAsync \n");
                        orderLists = orderDetails.ToList();

                    }
                    else
                    {
                        _logger.LogError("No orders found from DB in method GetAllOrdersAsync \n");
                    }

                    return orderLists;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching data for orders in method GetAllOrdersAsync : {ex.Message} \n");
                throw;
            }
        }
    }
}
