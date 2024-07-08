using Dapper;
using FruitStoreModels.Products;
using FruitStoreModels.Response;
using FruitStoreRepositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System.Data;
using System.Data.SqlClient;

namespace FruitStoreRepositories.Implementation
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductsRepository> _logger;
        public ProductsRepository(IConfiguration configuration, ILogger<ProductsRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        //... Method for delete product by product Id...//
        public async Task<int> DeleteProductByIdAsync(int productId)
        {
            try
            {
                _logger.LogInformation($"Deleting records for products starts in method DeleteProductByIdAsync Id : {productId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@ProductId", productId);
                    parameters.Add("@DeleteResult", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    _logger.LogInformation("Executing stored procedure stp_DeleteProductById \n");

                    await dbConnection.ExecuteAsync(
                        "stp_DeleteProductById",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    int deleteStatus = parameters.Get<int>("@DeleteResult");

                    if (deleteStatus == 1)
                    {
                        _logger.LogInformation($"Deleting details for products from DB succeeded in method DeleteProductByIdAsync for productId : {productId} \n");

                    }
                    else
                    {
                        _logger.LogError($"Products Deletion failed  from DB in method DeleteProductByIdAsync ProductId : {productId} \n");
                    }
                    return deleteStatus;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while Deleting data for products in method DeleteProductByIdAsync for productId : {productId} : {ex.Message} \n");
                throw;
            }

        }

        //... Method for get all products details...//
        public async Task<List<ProductsDetails>> GetAllProductsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching details for products starts in method GetAllProductsAsync \n");

                List<ProductsDetails> productsLists = null;

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    _logger.LogInformation("Executing stored procedure stp_GetAllProducts \n");

                    var productsDetails = await dbConnection.QueryAsync<ProductsDetails>(
                        "stp_GetAllProducts",
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (productsDetails.Any())
                    {
                        _logger.LogInformation("Fetching details for products from DB succeeded in method GetAllProductsAsync \n");
                        productsLists = productsDetails.ToList();
                        foreach (var product in productsLists)
                        {

                            string base64String = Convert.ToBase64String(product.ProductImage);
                            product.ImageData = base64String;
                            product.ProductImage = null;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No products found from DB in method GetAllProductsAsync \n");
                    }

                    return productsLists;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching data for products in method GetAllProductsAsync : {ex.Message} \n");
                throw;
            }
        }

        //... Method for insert update products details...//
        public async Task<int> InsertUpdateProductsAsync(AddUpdateProductsDetails addUpdateProduct)
        {
            try
            {
                bool convertedProductIdSuccess = false;

                bool convertedAdminIdSuccess = false;

                _logger.LogInformation($"Inserting-updating details for products starts in method InsertUpdateProductsAsync for product name : {addUpdateProduct.ProductName} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    _logger.LogInformation("Extracting file name from file \n");

                    string fileName = string.Empty;

                    if (addUpdateProduct.ImageData != null)
                    {
                        fileName = addUpdateProduct.ImageData.FileName;
                    }

                    _logger.LogInformation("Converting byte array to image to insert starts \n");

                    byte[] imageData;

                    using (var memoryStream = new MemoryStream())
                    {
                        addUpdateProduct.ImageData.CopyTo(memoryStream);
                        imageData = memoryStream.ToArray();
                    }

                    _logger.LogInformation("Converting byte array to image to insert end \n");
                    var parameters = new DynamicParameters();

                    _logger.LogInformation("Converting string productId to int starts \n");

                    int number = 0;
                    // Validation for product Id //
                    if (addUpdateProduct.ProductId != null)
                    {
                        // Attempt to parse the string into an integer //
                        convertedProductIdSuccess = int.TryParse(addUpdateProduct.ProductId, out number);
                        if (convertedProductIdSuccess)
                        {
                            _logger.LogInformation("Converting string productId to int completed \n");
                            parameters.Add("@ProductId", number);
                        }
                    }
                    else
                    {
                        parameters.Add("@ProductId", 0);
                    }
                    if (addUpdateProduct.AdminId != null)
                    {
                        // Attempt to parse the string into an integer //
                        convertedAdminIdSuccess = int.TryParse(addUpdateProduct.AdminId, out number);
                        if (convertedAdminIdSuccess)
                        {
                            _logger.LogInformation("Converting string admin Id to int completed \n");
                            parameters.Add("@AdminId", number);
                        }
                    }
                    parameters.Add("@CategoryId", addUpdateProduct.CategoryId);
                    parameters.Add("@ProductName", addUpdateProduct.ProductName);
                    parameters.Add("@ProductDescription", addUpdateProduct.ProductDescription);
                    parameters.Add("@Price", addUpdateProduct.Price);
                    parameters.Add("@StockQuantity", addUpdateProduct.StockQuantity);
                    parameters.Add("@ProductImage", imageData);
                    parameters.Add("@ImageName", fileName);

                    // Add output parameter for @ProductId
                    parameters.Add("@InsertedProductId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    _logger.LogInformation("Executing stored procedure stp_SetProductDetails...");

                    await dbConnection.ExecuteScalarAsync<int>(
                        "stp_SetProductDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    int productId = parameters.Get<int>("@InsertedProductId");

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (productId != 0)
                    {
                        _logger.LogInformation($"Product add update successfully from DB in method InsertUpdateProductsAsync for product name : {addUpdateProduct.ProductName} \n");
                    }
                    else
                    {
                        _logger.LogError($"Problem while inserting updating data to the DB in method InsertUpdateProductsAsync for product name : {addUpdateProduct.ProductName} \n");
                    }
                    return productId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while inserting updating data product details in method InsertUpdateProductsAsync : {addUpdateProduct.ProductName} : {ex.Message} \n");
                throw;
            }

        }

    }
}
