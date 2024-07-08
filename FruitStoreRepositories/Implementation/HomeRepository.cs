using CommonHelperUtility;
using Dapper;
using FruitStoreModels.Home;
using FruitStoreRepositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;

namespace FruitStoreRepositories.Implementation
{
    public class HomeRepository : IHomeRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeRepository> _logger;

        public HomeRepository(IConfiguration configuration, ILogger<HomeRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        //... Method for get best seller products...//
        public async Task<List<BestSellerProducts>> GetBestSellerProductsDetailsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching best seller products details starts in method GetBestSellerProductsDetailsAsync \n");

                List<BestSellerProducts> bestSellerProductsDetails = null;

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");


                    _logger.LogInformation("Executing stored procedure stp_GetBestSellerProducts \n");

                    var bestSellProductsDetails = await dbConnection.QueryAsync<BestSellerProducts>(
                        "stp_GetBestSellerProducts",
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (bestSellProductsDetails.Any())
                    {
                        _logger.LogInformation("Fetching details for best seller products from DB succeeded in method GetBestSellerProductsDetailsAsync \n");
                        bestSellerProductsDetails = bestSellProductsDetails.ToList();
                        foreach (var product in bestSellerProductsDetails)
                        {
                            string imageData = UserValidator.ConvertImageTo64(product.ProductImage);
                            product.Image = imageData;
                            product.ProductImage = null;
                        }
                    }
                    else
                    {
                        _logger.LogError("No best seller products found from DB in method GetBestSellerProductsDetailsAsync \n");
                    }

                    return bestSellerProductsDetails;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching best seller products data in method GetBestSellerProductsDetailsAsync : {ex.Message} \n");
                throw;
            }
        }

        //... Method for get top products...//
        public async Task<List<TopProductsDetails>> GetTopProductsDetailsAsync(string vegOrFruits)
        {
            try
            {
                _logger.LogInformation("Fetching top products details starts in method GetTopProductsDetailsAsync \n");

                List<TopProductsDetails> vegFruitDetails = null;

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new
                    {
                        FruitOrVeg = vegOrFruits
                    };

                    _logger.LogInformation("Executing stored procedure stp_GetTopProducts \n");

                    var topProductsDetails = await dbConnection.QueryAsync<TopProductsDetails>(
                        "stp_GetTopProducts",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (topProductsDetails.Any())
                    {
                        _logger.LogInformation("Fetching details for top products from DB succeeded in method GetTopProductsDetailsAsync \n");
                        vegFruitDetails = topProductsDetails.ToList();
                        foreach (var product in vegFruitDetails)
                        {
                            string imageData = UserValidator.ConvertImageTo64(product.ProductImage);
                            product.Image = imageData;
                        }
                    }
                    else
                    {
                        _logger.LogError("No products found from DB in method GetTopProductsDetailsAsync \n");
                    }

                    return vegFruitDetails;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching top products data in method GetTopProductsDetailsAsync : {ex.Message} \n");
                throw;
            }

        }
    }
}
