using Dapper;
using FruitStoreModels.Products;
using FruitStoreModels.ShopDetails;
using FruitStoreRepositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;

namespace FruitStoreRepositories.Implementation
{
    public class ShopDetailsRepository : IShopDetailsRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ShopDetailsRepository> _logger;

        public ShopDetailsRepository(IConfiguration configuration, ILogger<ShopDetailsRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        //...Method for check product purchasing...//
        public async Task<int> CheckProductPurchasingAsync(int UserId, int productId)
        {
            try
            {
                _logger.LogInformation($"Check product purchasing starts in method CheckProductPurchasingAsync for userId : {UserId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection closed successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", UserId);
                    parameters.Add("@ProductId", productId);
                    parameters.Add("@IsPurchased", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    _logger.LogInformation("Executing stored procedure stp_CheckProductPurchasing \n");

                    await dbConnection.ExecuteScalarAsync<int>(
                       "stp_CheckProductPurchasing",
                       parameters,
                       commandType: CommandType.StoredProcedure
                   );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    int purchasedId = parameters.Get<int>("@IsPurchased");

                    if (purchasedId != 0)
                    {
                        _logger.LogInformation($"Check product purchasing complted in method CheckProductPurchasingAsync for userId : {UserId} \n");

                    }
                    else
                    {
                        _logger.LogError($"Check product purchasing failed in method CheckProductPurchasingAsync for userId : {UserId} \n");
                    }

                    return purchasedId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while Check product purchasing in method CheckProductPurchasingAsync for userId : {UserId} :{ex.Message} \n");
                throw;
            }
        }

        //...Method for get product and it's respective rating...//
        public async Task<(List<RatingDetails>, ProductDetail)> GetProductAndRatingDetailsAsync(int productId)
        {
            try
            {
                _logger.LogInformation($"Fetching details for products and ratings has started in method GetProductAndRatingDetailsAsync : {productId} \n");

                List<RatingDetails> ratingDetails = null;

                ProductDetail productDetail = null;

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new
                    {
                        ProductId = productId
                    };

                    _logger.LogInformation("Executing stored procedure stp_GetProductAndReviews \n");

                    var result = await dbConnection.QueryMultipleAsync("stp_GetProductAndReviews", parameters, commandType: CommandType.StoredProcedure);

                    // Read the first result set
                    var productsDetails = await result.ReadAsync<ProductDetail>();

                    // Read the second result set
                    var rateDetails = await result.ReadAsync<RatingDetails>();

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (productsDetails.Any())
                    {
                        _logger.LogInformation($"Fetching details for products succeeded in method GetProductAndRatingDetailsAsync : {productId} \n");
                        productDetail = productsDetails.FirstOrDefault();
                        string base64String = Convert.ToBase64String(productDetail.ProductImage);
                        productDetail.Image = base64String;
                        productDetail.ProductImage = null;
                    }
                    else
                    {
                        _logger.LogError($"No Details found for products in method GetProductAndRatingDetailsAsync : {productId} \n");
                    }
                    if (rateDetails.Any())
                    {
                        _logger.LogInformation($"Fetching details for ratings succeeded in method GetProductAndRatingDetailsAsync : {productId} \n");
                        ratingDetails = rateDetails.ToList();
                    }
                    else
                    {
                        _logger.LogError($"No Details found for ratings in method GetProductAndRatingDetailsAsync : {productId} \n");
                    }
                    return (ratingDetails, productDetail);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"Error while fetching data for products and ratings in method GetProductAndRatingDetailsAsync : {productId} : {ex.Message} \n");
                throw;
            }
        }

        //...Method for add product comments and it's respective rating...//
        public async Task<int> PostProductCommentAsync(AddProductRatings rating)
        {
            try
            {
                _logger.LogInformation($"Posting product comments and it's rating starts in method PostProductCommentAsync for userId : {rating.UserId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection closed successfully \n");

                    var parameters = new
                    {
                        Review = rating.Review,
                        Rate = rating.Rate,
                        UserId = rating.UserId,
                        ProductId = rating.ProductId
                    };

                    _logger.LogInformation("Executing stored procedure stp_SetProductRating \n");

                    var productRatingId = await dbConnection.ExecuteScalarAsync<int>(
                        "stp_SetProductRating",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");


                    if (productRatingId != 0)
                    {
                        _logger.LogInformation($"Posting product comments and it's rating completed in method PostProductCommentAsync for userId : {rating.UserId} \n");

                    }
                    else
                    {
                        _logger.LogError($"Posting product comments and it's rating failed in method PostProductCommentAsync for userId : {rating.UserId} \n");
                    }

                    return productRatingId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while posting product comments and it's rating in method PostProductCommentAsync for userId : {rating.UserId} :{ex.Message} \n");
                throw;
            }
        }
    }
}
