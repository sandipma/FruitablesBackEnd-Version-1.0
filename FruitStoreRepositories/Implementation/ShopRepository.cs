using CommonHelperUtility;
using Dapper;
using FruitStoreModels.Products;
using FruitStoreModels.Shop;
using FruitStoreRepositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;

namespace FruitStoreRepositories.Implementation
{
    public class ShopRepository : IShopRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ShopRepository> _logger;
        public ShopRepository(IConfiguration configuration, ILogger<ShopRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        //...Method for fetching all shop details...//
        public async Task<List<AllShopDetails>> GetAllShopDetailsAsync(ShopFilterCriterio filterCriterio)
        {
            try
            {
                _logger.LogInformation("Fetching details for all shop details starts in method GetAllShopDetailsAsync \n");

                List<AllShopDetails> allShopLists = null;

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    _logger.LogInformation("Executing stored procedure stp_GetAllShopDetails \n");

                    var parameters = new DynamicParameters();

                    parameters.Add("@CategoryName", filterCriterio.CategoryName);
                    parameters.Add("@PriceValue", filterCriterio.PriceValue);
                    parameters.Add("@SortValue", filterCriterio.SortValue);

                    var allShopDetails = await dbConnection.QueryAsync<AllShopDetails>(
                        "stp_GetAllShopDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (allShopDetails.Any())
                    {
                        _logger.LogInformation("Fetching details for all shop details from DB succeeded in method GetAllShopDetailsAsync \n");
                        allShopLists = allShopDetails.ToList();
                        foreach (var product in allShopLists)
                        {

                            string base64String = Convert.ToBase64String(product.ProductImage);
                            product.ImageData = base64String;
                            product.ProductImage = null;
                        }
                    }
                    else
                    {
                        _logger.LogError("No all shop details found from DB in method GetAllShopDetailsAsync \n");
                    }

                    return allShopLists;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching data for all shop details in method GetAllShopDetailsAsync : {ex.Message} \n");
                throw;
            }
        }
    }
}
