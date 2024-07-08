using CommonHelperUtility;
using FruitStoreModels.Products;
using FruitStoreModels.Response;
using FruitStoreModels.Shop;
using FruitStoreRepositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FruitStoreWebAPI.Controllers
{
    /// <summary>
    /// Represents a class that handles shop operations.
    /// </summary>
    //[CustomAuthorizeAttribute("user")]
    [Route("api/[controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly ILogger<ShopController> _logger;
        private readonly IShopRepository _shopRepository;

        /// <summary>
        /// Represents a class that handles shop operations.
        /// </summary>
        public ShopController(ILogger<ShopController> logger, IShopRepository shopRepository)
        {
            _logger = logger;
            _shopRepository = shopRepository;
        }

        /// <summary>
        /// Retrieves details for all shop details.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves all shop details for all products available in the system.
        /// </remarks>
        /// <returns>Returns a response containing details for shop details.</returns>
        /// <response code="200">Returns a successful response along with all shop product details.</response>    
        /// <response code="400">Returns a 400 bad request</response>  
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     GET /api/shop/get-all-shop-details
        /// 
        /// </example>
        [HttpGet("get-all-shop-details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<ProductsDetails>>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetAllShopDetails([FromQuery] ShopFilterCriterio shopFilterCriterio)
        {
            _logger.LogInformation("Validation started in controller action method GetAllShopDetails \n");

            _logger.LogInformation("Category name validation started \n");

            if (string.IsNullOrEmpty(shopFilterCriterio.CategoryName))
            {
                _logger.LogError("Category is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "Category is required"));
            }
            _logger.LogInformation("Category name validation passed \n");

            _logger.LogInformation("PriceValue validation started \n");

            if (shopFilterCriterio.PriceValue == 0)
            {
                _logger.LogError("PriceValue is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "PriceValue is required"));
            }
            _logger.LogInformation("PriceValue validation passed \n");

            _logger.LogInformation("SortValue validation started \n");

            if (string.IsNullOrEmpty(shopFilterCriterio.SortValue))
            {
                _logger.LogError("SortValue is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "SortValue is required"));
            }
            _logger.LogInformation("SortValue validation passed \n");

            _logger.LogInformation("Validation completed in controller action method GetAllShopDetails \n");

            try
            {
                _logger.LogInformation("Fetching details for all shop details starts in controller action method GetAllShopDetails \n");

                var shopDetails = await _shopRepository.GetAllShopDetailsAsync(shopFilterCriterio);

                if (shopDetails != null)
                {
                    _logger.LogInformation("Fetching details all shop details succeeded in controller action method GetAllShopDetails \n");
                    return StatusCode(200, new ApiResponse<List<AllShopDetails>>(200, "All shop details..", shopDetails));
                }
                else
                {
                    _logger.LogInformation("Fetching details for all shop details failed no products availabe in controller action method GetAllShopDetails \n");
                    return StatusCode(200, new ApiResponse<List<AllShopDetails>>(200, "No products available", null));
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    if (sqlEx.Message.Contains("Category can not be null."))
                    {
                        _logger.LogError($"Error during all shop details fetching in controller action method GetAllShopDetails {sqlEx.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Category can not be null"));
                    }
                    else if (sqlEx.Message.Contains("Price can not be null."))
                    {
                        _logger.LogError($"Error during all shop details fetching in controller action method GetAllShopDetails {sqlEx.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Price can not be null"));
                    }
                    else if (sqlEx.Message.Contains("Sorting can not be null"))
                    {
                        _logger.LogError($"Error during all shop details fetching in controller action method GetAllShopDetails {sqlEx.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Sorting can not be null"));
                    }
                }
                _logger.LogError($"SQL Error during all shop details fetching in controller action method GetAllShopDetails : {sqlEx.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during all shop details fetching in controller action method GetAllShopDetails : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }
    }
}
