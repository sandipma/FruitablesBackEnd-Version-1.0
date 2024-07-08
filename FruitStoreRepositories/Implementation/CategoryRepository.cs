using Dapper;
using FruitStoreModels.Category;
using FruitStoreRepositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;

namespace FruitStoreRepositories.Implementation
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(IConfiguration configuration, ILogger<CategoryRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        //...Method to delete category by category Id...//
        public async Task<int> DeleteCategoryByIdAsync(int categoryId)
        {
            try
            {
                _logger.LogInformation($"Deleting records for category starts method DeleteCategoryAsync for categoryId : {categoryId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();

                    parameters.Add("@CategoryId", categoryId);
                    parameters.Add("@DeleteResult", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    _logger.LogInformation("Executing stored procedure stp_DeleteCategoryById \n");

                    await dbConnection.ExecuteAsync(
                        "stp_DeleteCategoryById",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    int deleteStatus = parameters.Get<int>("@DeleteResult");

                    if (deleteStatus == 0)
                    {
                        _logger.LogError($"Category deletion from DB failed in method DeleteCategoryAsync for categoryId : {categoryId} \n");
                    }
                    else
                    {
                        _logger.LogInformation($"Category deletion from DB completed successfully in method DeleteCategoryAsync for categoryId : {categoryId} \n");
                    }

                    return deleteStatus;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while deleting category details in method DeleteCategoryAsync for categoryId : {categoryId} : {ex.Message} \n");
                throw;
            }
        }

        //...Method to get all categories ...//
        public async Task<List<CategoryDetails>> GetAllCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching details for categories starts in method GetAllCategoriesAsync \n");

                List<CategoryDetails> catDetails = null;

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    _logger.LogInformation("Executing stored procedure stp_GetAllCategories \n");

                    var categories = await dbConnection.QueryAsync<CategoryDetails>(
                        "stp_GetAllCategories",
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (categories.Any())
                    {
                        _logger.LogInformation("Fetching details for categories from DB succeeded in method GetAllCategoriesAsync \n");

                        catDetails = categories.ToList();
                    }
                    else
                    {
                        _logger.LogError("No categories found from DB in method GetAllCategoriesAsync \n");

                    }
                    return catDetails;
                }
            }

            catch (Exception ex)
            {

                _logger.LogError($"Error while fetching data for category in method GetAllCategoriesAsync : {ex.Message} \n");
                throw;
            }
        }

        //...Method to get all categories with respective product count ...//
        public async Task<List<CategoryWithProductsCounts>> GetCategoryWithProductsCountsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching details for category products starts in method GetCategoryWithProductsCountsAsync \n");

                List<CategoryWithProductsCounts> catWithProductCount = null;

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    _logger.LogInformation("Executing stored procedure stp_GetCategoriesWithCount \n");

                    var catProdDetails = await dbConnection.QueryAsync<CategoryWithProductsCounts>(
                        "stp_GetCategoriesWithCount",
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (catProdDetails.Any())
                    {

                        _logger.LogInformation("Fetching details for category with products counts from DB succeeded in method GetCategoryWithProductsCountsAsync \n");

                        catWithProductCount = catProdDetails.ToList();
                    }
                    else
                    {
                        _logger.LogWarning("No Category and products count from DB found in method GetCategoryWithProductsCountsAsync \n");

                    }
                    return catWithProductCount;
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"Error while fetching data for category with  products count  in method GetCategoryWithProductsCountsAsync : {ex.Message} \n");
                throw;
            }
        }

        //...Method for insert new category...//
        public async Task<int> InsertNewCategoryAsync(AddCategoryDetails addCategoryDetails)
        {
            try
            {
                _logger.LogInformation($"Inserting category data in method InsertNewCategoryAsync starts for category : {addCategoryDetails.CategoryName} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection closed successfully \n");

                    var parameters = new
                    {
                        AdminId = addCategoryDetails.AdminId,   
                        CategoryName = addCategoryDetails.CategoryName,
                        CategoryDescription = addCategoryDetails.CategoryDescription
                    };

                    _logger.LogInformation("Executing stored procedure stp_SetCategoryDetails \n");

                    var userId = await dbConnection.ExecuteScalarAsync<int>(
                        "stp_SetCategoryDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation($"Stored procedure completed successfully \n");

                    if (userId != 0)
                    {
                        _logger.LogInformation($"Inserting category data in DB  for method InsertNewCategoryAsync completed successfully for category : {addCategoryDetails.CategoryName} \n");
                    }
                    else
                    {
                        _logger.LogError($"Inserting category data in DB for method InsertNewCategoryAsync failed for category : {addCategoryDetails.CategoryName} \n");
                    }

                    return userId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while inserting category data in method InsertNewCategoryAsync for category : {addCategoryDetails.CategoryName} : {ex.Message} \n");
                throw;
            }
        }
    }
}
