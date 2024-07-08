using Dapper;
using FruitStoreModels.Cart;
using FruitStoreRepositories.Interfaces;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FruitStoreRepositories.Implementation
{
    public class CartDetailsRepository : ICartDetailsRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductsRepository> _logger;
        public CartDetailsRepository(IConfiguration configuration, ILogger<ProductsRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        //... Method for delete cart item by cart Id...//
        public async Task<int> DeleteCartItemByIdAsync(int cartId)
        {
            try
            {
                _logger.LogInformation($"Deleting records for cart item by Id starts in method DeleteCartItemByIdAsync Id : {cartId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@CartId", cartId);
                    parameters.Add("@DeleteResult", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    _logger.LogInformation("Executing stored procedure stp_DeleteCartItemById \n");

                    await dbConnection.ExecuteAsync(
                        "stp_DeleteCartItemById",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    int deleteStatus = parameters.Get<int>("@DeleteResult");

                    if (deleteStatus == 1)
                    {
                        _logger.LogInformation($"Deleting details for cart item by Id from DB succeeded in method DeleteCartItemByIdAsync for cartId : {cartId} \n");

                    }
                    else
                    {
                        _logger.LogError($"Cart item by Id deletion failed from DB in method DeleteCartItemByIdAsync for cartId : {cartId} \n");
                    }
                    return deleteStatus;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while deleting data for cart item by Id in method DeleteCartItemByIdAsync for cartId : {cartId} : {ex.Message} \n");
                throw;
            }
        }

        //... Method for get all products cart details...//
        public async Task<(List<CartDetails>, PriceDetails)> GetProductsCartDetailsByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Fetching details for all products cart details with total starts in method GetProductsCartDetailsByUserIdAsync for user Id : {userId} \n");

                List<CartDetails> cartDetails = null;

                PriceDetails priceDetail = null;

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    _logger.LogInformation("Executing stored procedure stp_GetProductsCartDetailsByUserId \n");

                    var parameters = new
                    {
                        UserId = userId,
                    };
                    var result = await dbConnection.QueryMultipleAsync("stp_GetProductsCartDetailsByUserId", parameters, commandType: CommandType.StoredProcedure);

                    // Read the first result set //
                    var currentCartDetails = await result.ReadAsync<CartDetails>();

                    // Read the second result set //
                    var currentPriceDetails = await result.ReadAsync<PriceDetails>();

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (currentCartDetails.Any() && currentPriceDetails.Any())
                    {
                        cartDetails = currentCartDetails.ToList();
                        priceDetail = currentPriceDetails.FirstOrDefault();
                        _logger.LogInformation($"Fetching details for all products cart details with total from DB succeeded in method GetProductsCartDetailsByUserIdAsync for user Id : {userId} \n");
                    }
                    else
                    {
                        _logger.LogError($"No products cart with total details found from DB in method GetProductsCartDetailsByUserIdAsync for user Id : {userId} \n");
                    }

                    return (cartDetails, priceDetail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching data for products cart details with in method GetProductsCartDetailsByUserIdAsync for user Id : {userId} : {ex.Message} \n");
                throw;
            }
        }

        //... Method for get bag counter of products cart ...//
        public async Task<BagCounterDetails> GetBagCounterDetailsByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Fetching records for bag counter by user Id starts in method GetBagCounterDetailsByUserIdAsync Id : {userId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);

                    _logger.LogInformation("Executing stored procedure stp_GetBagCounterByUserId \n");

                    var bagCounterDetails = await dbConnection.QueryFirstOrDefaultAsync<BagCounterDetails>(
                        "stp_GetBagCounterByUserId",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (bagCounterDetails != null)
                    {
                        _logger.LogInformation($"Fetching records for bag counter by user Id from DB succeeded in method GetBagCounterDetailsByUserIdAsync for Id : {userId} \n");

                    }
                    else
                    {
                        _logger.LogError($"Fetching records for bag counter by user Id from DB not found in method GetBagCounterDetailsByUserIdAsync for Id : {userId} \n");
                    }
                    return bagCounterDetails;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching records for bag counter by user Id in method GetBagCounterDetailsByUserIdAsync for Id : {userId} : {ex.Message} \n");
                throw;
            }
        }

        //... Method for insert cart details...//
        public async Task<int> InsertCartDetailsAsync(AddCartDetails addCartItem)
        {
            try
            {

                _logger.LogInformation($"Inserting cart details for products starts in method InsertCartDetailsAsync for product name : {addCartItem.ProductName} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();

                    parameters.Add("@ProductImage", addCartItem.ProductImage);
                    parameters.Add("@ProductName", addCartItem.ProductName);
                    parameters.Add("@Price", addCartItem.Price);
                    parameters.Add("@SubTotal ", addCartItem.SubTotal);
                    parameters.Add("@UserId", addCartItem.UserId);
                    parameters.Add("CurrentQuant", addCartItem.CurrentQuant);
                    parameters.Add("@InsertedCardId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    _logger.LogInformation("Executing stored procedure stp_SetCartDetails...");

                    await dbConnection.ExecuteScalarAsync<int>(
                        "stp_SetCartDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    int cartId = parameters.Get<int>("@InsertedCardId");

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (cartId != 0)
                    {
                        _logger.LogInformation($"Cart product details add successfully from DB in method InsertCartDetailsAsync for product name : {addCartItem.ProductName} \n");
                    }
                    else
                    {
                        _logger.LogError($"Cart product details inserting data to the DB failed in method InsertCartDetailsAsync for product name : {addCartItem.ProductName} \n");
                    }
                    return cartId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while inserting cart product details data in method InsertCartDetailsAsync : {addCartItem.ProductName} : {ex.Message} \n");
                throw;
            }
        }

        //... Method for update specific cart details...//
        public async Task<int> UpdateCartDetailsAsync(UpdateCartDetails updateCartItem)
        {
            try
            {
                _logger.LogInformation($"Updating cart details for products starts in method UpdateCartDetailsAsync for cart Id : {updateCartItem.CartId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");
                    var parameters = new DynamicParameters();
                    parameters.Add("@CartId", updateCartItem.CartId);
                    parameters.Add("@Price", updateCartItem.Price);
                    parameters.Add("@Quantity", updateCartItem.Quantity);
                    parameters.Add("@ProductName", updateCartItem.ProductName);
                    parameters.Add("@SubTotal ", updateCartItem.SubTotal);
                    parameters.Add("@UserId", updateCartItem.UserId);
                    parameters.Add("@TotalQuant", updateCartItem.TotalQuant);
                    parameters.Add("@IncreseOrDecrese", updateCartItem.IncreaseOrDecrease);
                    parameters.Add("@UpdatedCardId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    _logger.LogInformation("Executing stored procedure stp_UpdateCartDetailsById...");

                    await dbConnection.ExecuteScalarAsync<int>(
                        "stp_UpdateCartDetailsById",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    int cartId = parameters.Get<int>("@UpdatedCardId");

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (cartId != 0)
                    {
                        _logger.LogInformation($"Cart product details update successfully from DB in method UpdateCartDetailsAsync for cart Id : {updateCartItem.CartId} \n");
                    }
                    else
                    {
                        _logger.LogError($"Cart product details updating data to the DB failed in method UpdateCartDetailsAsync for cart Id : {updateCartItem.CartId} \n");
                    }
                    return cartId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating cart product details data in method UpdateCartDetailsAsync for cart Id : {updateCartItem.CartId} : {ex.Message} \n");
                throw;
            }
        }

        //... Method for task schedular to delete cart details after 24 hours...//
        public async Task SetSchedularAsync()
        {
            try
            {
                _logger.LogInformation($"Deleting records for cart item by Id starts in method DeleteCartItemByIdAsync Id :  \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    _logger.LogInformation("Executing stored procedure stp_DeleteCartItemById \n");

                    await dbConnection.ExecuteAsync(
                        "stp_SetSchedular",
                        commandType: CommandType.StoredProcedure
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while deleting data for cart item by Id in method DeleteCartItemByIdAsync for cartId :  : {ex.Message} \n");
                throw;
            }
        }
    }
}
