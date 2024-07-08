using CommonHelperUtility;
using FruitStoreModels.Products;
using FruitStoreModels.Response;
using FruitStoreModels.ShopDetails;
using FruitStoreRepositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FruitStoreWebAPI.Controllers
{
    /// <summary>
    /// Represents a class that handles shop details operations.
    /// </summary>
    [CustomAuthorizeAttribute("user")]
    [Route("api/[controller]")]
    [ApiController]
    public class ShopDetailsController : ControllerBase
    {
        private readonly IShopDetailsRepository _shopDetailsRepository;
        private readonly ILogger<ShopDetailsController> _logger;

        /// <summary>
        /// Represents a class that handles shop details operations.
        /// </summary>
        public ShopDetailsController(ILogger<ShopDetailsController> logger, IShopDetailsRepository shopDetailsRepository)
        {

            _logger = logger;
            _shopDetailsRepository = shopDetailsRepository;

        }
        /// <summary>
        /// Adds a new product rating.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/shop/productratings
        ///     {
        ///        "productId": 1,
        ///        "userId": 123,
        ///        "review": "This is a great product!",
        ///        "rating": 5
        ///     }
        ///
        /// </remarks>
        /// <param name="addProductsRating">Object containing product rating details.</param>
        /// <returns>The newly added product rating.</returns>
        /// <response code="201">Returns when the product rating is added successfully.</response>
        /// <response code="400">Returns when there's a validation error in the request payload.</response>
        /// <response code="404">Returns when a required resource (e.g., user session, product) is not found.</response>
        /// <response code="500">Returns when there's a server error during the insertion process.</response>
        [HttpPost("productratings")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> PostProductRating(AddProductRatings addProductsRating)
        {
            _logger.LogInformation("Validation started in controller action method PostProductRating \n");

            _logger.LogInformation("Review validation started \n");

            if (string.IsNullOrEmpty(addProductsRating.Review))
            {
                _logger.LogError("Review name is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Review is required"));
            }
            _logger.LogInformation("Review validation passed \n");

            _logger.LogInformation("Rate validation started \n");

            if (addProductsRating.Rate == 0)
            {
                _logger.LogError("Rating is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Rating is required"));
            }
            _logger.LogInformation("Rate validation passed \n");

            _logger.LogInformation("UserId validation started \n");

            if (addProductsRating.UserId == 0)
            {
                _logger.LogError("UserId is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid user, kindly check & try again"));
            }
            _logger.LogInformation("UserId validation passed \n");

            _logger.LogInformation("ProductId validation started \n");

            if (addProductsRating.ProductId == 0)
            {
                _logger.LogError("ProductId is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "ProductId is required"));
            }
            _logger.LogInformation("ProductId validation passed \n");

            _logger.LogInformation("Validation passed in controller action method PostProductRating \n");

            try
            {
                _logger.LogInformation($"Processing insertion of new product rating details in controller action method PostProductRating for userId :{addProductsRating.UserId} \n");

                var productRatingDetails = await _shopDetailsRepository.PostProductCommentAsync(addProductsRating);

                if (productRatingDetails != 0)
                {
                    _logger.LogInformation($"New product rating added successfully in controller action method PostProductRating for userId :{addProductsRating.UserId} \n");
                    return StatusCode(201, new ApiResponse<string>(201, "Comment and rating added successfully."));
                }

                _logger.LogError($"Problem while product rating in controller action method PostProductRating for userId :{addProductsRating.UserId} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {

                    if (sqlException.Message.Contains("Review cannot be NULL or empty."))
                    {
                        _logger.LogError($"Problem while product rating in controller action method PostProductRating for userId :{addProductsRating.UserId} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Review cannot be NULL or empty"));
                    }
                    else if (sqlException.Message.Contains("UserId cannot be NULL or empty."))
                    {
                        _logger.LogError($"Problem while product rating in controller action method PostProductRating for userId :{addProductsRating.UserId} : {sqlException.Message} \n");
                        return StatusCode(404, new ApiResponse<string>(404, "We couldn't find a valid login session. Please verify your credentials and try again."));
                    }
                    else if (sqlException.Message.Contains("Rate is required."))
                    {
                        _logger.LogError($"Problem while product rating in controller action method PostProductRating for userId :{addProductsRating.UserId} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Rate is required."));
                    }
                    else if (sqlException.Message.Contains("ProductId cannot be NULL or empty."))
                    {
                        _logger.LogError($"Problem while product rating in controller action method PostProductRating for userId :{addProductsRating.UserId} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid product data."));
                    }
                    else if (sqlException.Message.Contains("Invalid ProductId. Product with such ProductId does not exist."))
                    {
                        _logger.LogError($"Problem while product rating in controller action method PostProductRating for userId :{addProductsRating.UserId} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid product data."));
                    }
                }
                _logger.LogError(sqlException, $"SQL Error during product and rating details in controller action method PostProductRating for userId :{addProductsRating.UserId} : {sqlException.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during product and rating details insertion  in controller action method PostProductRating for userId :{addProductsRating.UserId} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
        }

        /// <summary>
        /// Retrieves details for a specific product including its rating.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/shop/products-and-rating/1
        ///     {}
        /// </remarks>
        /// <param name="productId">The ID of the product for which details are to be retrieved.</param>
        /// <response code="200">Returns an OK response with product and rating details if retrieval is successful.</response>
        /// <response code="400">Returns a Bad Request response for validation errors or invalid data.</response>
        /// <response code="404">Returns a Not Found response if the specified product or rating details are not found.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during retrieval.</response>
        [HttpGet("products-and-rating/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ProductRatingDetails>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetProductsAndRating([FromRoute] int productId)
        {
            _logger.LogInformation("Validation started in controller action method GetProductsAndRating \n");

            _logger.LogInformation("ProductId validation started \n");

            if (productId == 0)
            {
                _logger.LogError("ProductId is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "ProductId is required."));
            }
            _logger.LogInformation("ProductId validation passed \n");

            _logger.LogInformation("Validation started in controller action method GetProductsAndRating \n");

            try
            {
                _logger.LogInformation($"Fetching details for products and rating starts in controller action method GetProductsAndRating for productId : {productId} \n");

                var productAndRatingDetails = await _shopDetailsRepository.GetProductAndRatingDetailsAsync(productId);

                var response = new ProductRatingDetails
                {
                    RetDetails = productAndRatingDetails.Item1,
                    ProdDetails = productAndRatingDetails.Item2
                };
                if ((response.ProdDetails != null) || (response.RetDetails != null))
                {
                    _logger.LogInformation($"Fetching details for products and rating succeeded in controller action method GetProductsAndRating for productId : {productId} \n");
                    return StatusCode(200, new ApiResponse<ProductRatingDetails>(200, "Products rating retrieved successfully.", response));
                }

                _logger.LogError($"No products rating found in controller action method GetProductsAndRating for productId : {productId} \n");
                return StatusCode(200, new ApiResponse<ProductRatingDetails>(200, "Products rating not found.", null));
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("ProductId is required."))
                    {
                        _logger.LogError($"Fetching details for products and rating failed in controller action method GetProductsAndRating : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid product data try again."));
                    }
                    else if (sqlException.Message.Contains("ProductId must be a positive integer."))
                    {
                        _logger.LogError($"Fetching details for products and rating failed in controller action method GetProductsAndRating : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid product data try again."));
                    }
                    else if (sqlException.Message.Contains("Invalid ProductId. Product with such ProductId does not exist."))
                    {
                        _logger.LogError($"Fetching details for products and rating failed in controller action method GetProductsAndRating : {sqlException.Message} \n");
                        return StatusCode(404, new ApiResponse<string>(404, "Product not exists"));
                    }
                }
                _logger.LogError($"SQL Error during product and rating details retrieval in controller action method GetProductsAndRating : {sqlException.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during product and rating details retrieval in controller action method GetProductsAndRating : {productId} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }

        /// <summary>
        /// Checks whether a product has been purchased by a user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/shop/purchasing?userId=123&productId=456
        ///
        /// </remarks>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product to check.</param>
        /// <returns>A response indicating whether the product has been purchased by the user.</returns>
        /// <response code="200">Returns when the product has been purchased by the user.</response>
        /// <response code="400">Returns when the userId or productId is not provided.</response>
        /// <response code="404">Returns when the product has not been purchased by the user.</response>
        /// <response code="500">Returns when there's a server error during the operation.</response>
        [HttpGet("check-purchasing")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> CheckProductPurchasing(int userId, int productId)
        {
            _logger.LogInformation("Validation started in controller action method CheckProductPurchasing \n");

            _logger.LogInformation("ProductId validation started \n");

            if (productId == 0)
            {
                _logger.LogError("ProductId is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "ProductId is required."));
            }
            _logger.LogInformation("ProductId validation passed \n");

            _logger.LogInformation("userId validation started \n");

            if (userId == 0)
            {
                _logger.LogError("userId is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "Invalid user..kindly check & try again"));
            }
            _logger.LogInformation("userId validation passed \n");

            _logger.LogInformation("Validation passed in controller action method CheckProductPurchasing \n");

            try
            {
                _logger.LogInformation($"Checking product purchasing starts in controller action method CheckProductPurchasing for userId : {userId} \n");

                var productPurchsingStatus = await _shopDetailsRepository.CheckProductPurchasingAsync(userId, productId);

                if (productPurchsingStatus != 0)
                {
                    _logger.LogInformation($"Checking product purchasing completed in controller action method CheckProductPurchasing for userId : {userId} \n");
                    return StatusCode(200, new ApiResponse<string>(200, "Products purchased by user."));
                }

                _logger.LogError($"Checking product purchasing not found in controller action method CheckProductPurchasing for userId : {userId} \n");
                return StatusCode(400, new ApiResponse<string>(400, "You have not purchased this product..can't give rating"));

            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("ProductId cannot be NULL or empty."))
                    {
                        _logger.LogInformation($"Checking product purchasing failed in controller action method CheckProductPurchasing for userId : {userId} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid product data try again."));
                    }
                    else if (sqlException.Message.Contains("UserId Id cannot be NULL or empty."))
                    {
                        _logger.LogInformation($"Checking product purchasing failed in controller action method CheckProductPurchasing for userId : {userId} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid user."));
                    }
                    else if (sqlException.Message.Contains("Invalid ProductId. Product with such ProductId does not exist."))
                    {
                        _logger.LogInformation($"Checking product purchasing failed in controller action method CheckProductPurchasing for userId : {userId} \n");
                        return StatusCode(404, new ApiResponse<string>(404, "Product not exists"));
                    }
                }
                _logger.LogError($"SQL Error during check product purchasing in controller action method CheckProductPurchasing for userId : {userId}: {sqlException.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during check product purchasing in controller action method CheckProductPurchasing for userId : {userId}  : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }
    }
}
