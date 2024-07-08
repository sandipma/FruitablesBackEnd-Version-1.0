using FruitStoreModels.About_Us;
using FruitStoreModels.Products;
using FruitStoreRepositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Data;
using Dapper;

namespace FruitStoreRepositories.Implementation
{
    public class AboutUsRepository : IAboutUsRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AboutUsRepository> _logger;
        public AboutUsRepository(IConfiguration configuration, ILogger<AboutUsRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        //... Method for get overall product review ...//
        public async Task<OverAllReview> GetOverAllReviewAsync()
        {
            try
            {
                _logger.LogInformation("Fetching over all review details for products starts in method GetOverAllReviewAsync \n");
            
                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    _logger.LogInformation("Executing stored procedure stp_GetOverAllReview \n");

                    var overAllReviewDetails = await dbConnection.QueryFirstOrDefaultAsync<OverAllReview>(
                        "stp_GetOverAllReview",
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (overAllReviewDetails != null)
                    {
                         
                        _logger.LogInformation("Fetching over all review details for products from DB succeeded in method GetOverAllReviewAsync \n");
                     
                    }
                    else
                    {
                        _logger.LogError("No over all review found details for products found from DB in method GetOverAllReviewAsync \n");
                    }

                    return overAllReviewDetails;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching over all review details for products in method GetOverAllReviewAsync : {ex.Message} \n");
                throw;
            }
        }
    }
}
