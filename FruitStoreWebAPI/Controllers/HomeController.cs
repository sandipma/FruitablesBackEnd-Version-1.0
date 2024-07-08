using FruitStoreModels.Home;
using FruitStoreModels.Response;
using FruitStoreRepositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FruitStoreWebAPI.Controllers
{
    /// <summary>
    /// Represents a class that handles home screen operations.
    /// </summary>
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeRepository _homeRepository;

        /// <summary>
        /// Represents a class that handles home screen operations.
        /// </summary>
        public HomeController(ILogger<HomeController> logger, IHomeRepository homeRepository)
        {
            _logger = logger;
            _homeRepository = homeRepository;
        }

        /// <summary>
        /// Get Top Products
        /// </summary>
        /// <remarks>
        /// Retrieves the top products based on the specified category.
        /// </remarks>
        /// <param name="vegOrFruits">Category filter for retrieving top products. Use 'All' for all categories, 'V' for vegetables, and 'F' for fruits.</param>
        /// <returns>Returns a list of top products with details.</returns>
        /// <response code="200">Returns the list of top products with details.</response>
        /// <response code="400">Invalid category value. Please provide a valid category value ('All', 'V', 'F').</response>
        /// <response code="404">Returns not found response for top products.</response>
        /// <response code="500">Oops! Something went wrong on our end. Please try again later or contact support if the problem persists.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     GET /api/home/top-products?vegOrFruits=All
        /// 
        /// </example>
        [HttpGet("top-products")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<TopProductsDetails>>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetTopProducts(string vegOrFruits)
        {
            _logger.LogInformation("Validation started in controller action method GetTopProducts \n");

            _logger.LogInformation("VegOrFruits validation started \n");

            if (string.IsNullOrEmpty(vegOrFruits))
            {
                _logger.LogError("VegOrFruits parameter is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "Invalid Product.."));
            }
            _logger.LogInformation("VegOrFruits validation passed \n");

            _logger.LogInformation("Validation started in controller action method GetTopProducts \n");
            try
            {
                _logger.LogInformation($"Fetching details for all top products starts in controller action method GetTopProducts  \n");

                List<TopProductsDetails> vegFruitsDetails = null;

                vegFruitsDetails = await _homeRepository.GetTopProductsDetailsAsync(vegOrFruits);

                if (vegFruitsDetails != null && vegFruitsDetails.Any())
                {
                    _logger.LogInformation("Returning successful response with top products in controller action method GetTopProducts \n");
                    return StatusCode(200, new ApiResponse<List<TopProductsDetails>>(200, "Top products data retrieved successfully.", vegFruitsDetails));
                }
                else
                {
                    _logger.LogError($"Fetching details for all top products failed in controller action method GetTopProducts  \n");
                    return StatusCode(200, new ApiResponse<List<TopProductsDetails>>(200, "No top products data available.", vegFruitsDetails));
                }

            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException && sqlException.Number == 50000)
                {
                    if (ex.Message.Contains("FruitOrVeg must be one of the following values: All Products, Vegetables, Fruits"))
                    {
                        _logger.LogError($"Fetching details for all top products failed in controller action method GetTopProducts : {sqlException.Message}  \n");
                        return StatusCode(404, new ApiResponse<string>(404, "Category not exists"));
                    }
                    else
                    {
                        _logger.LogError($"SQL Error during all top products in controller action method GetTopProducts : {ex.Message} \n");
                        return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                    }
                }
                _logger.LogError($"Error during all top products in controller action method GetTopProducts : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
        }

        /// <summary>
        /// Get Best Seller Products
        /// </summary>
        /// <remarks>
        /// Retrieves the best seller products based.
        /// </remarks>
        /// <returns>Returns a list of best seller products with details.</returns>
        /// <response code="200">Returns the list of best seller products with details.</response>
        /// <response code="500">Oops! Something went wrong on our end. Please try again later or contact support if the problem persists.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     GET /api/home/bestseller-products
        /// 
        /// </example>
        [HttpGet("bestseller-products")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<BestSellerProducts>>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetBestSellerProducts()
        {
            try
            {
                _logger.LogInformation($"Fetching details for all best seller products starts in controller action method GetBestSellerProducts  \n");

                var bestSellerProductDetails = await _homeRepository.GetBestSellerProductsDetailsAsync();

                if (bestSellerProductDetails != null)
                {
                    _logger.LogInformation("Returning successful response with best seller products in controller action method GetBestSellerProducts \n");
                    return StatusCode(200, new ApiResponse<List<BestSellerProducts>>(200, "Best seller products data retrieved successfully.", bestSellerProductDetails));
                }
                else
                {
                    _logger.LogError($"Fetching details for all best seller products failed in controller action method GetBestSellerProducts  \n");
                    return StatusCode(200, new ApiResponse<List<BestSellerProducts>>(200, "No best seller products data available", null));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during all best seller products in controller action method GetBestSellerProducts : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
        }
    }
}
