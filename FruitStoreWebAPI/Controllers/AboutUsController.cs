using FruitStoreModels.About_Us;
using FruitStoreModels.Response;
using FruitStoreRepositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FruitStoreWebAPI.Controllers
{
    /// <summary>
    /// Represents a class that handles about us related operations.
    /// </summary>
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AboutUsController : ControllerBase
    {
        private readonly IAboutUsRepository _aboutUsRepository;
        private readonly ILogger<AboutUsController> _logger;

        /// <summary>
        /// Represents a class that handles about us related operations.
        /// </summary>
        public AboutUsController(ILogger<AboutUsController> logger, IAboutUsRepository aboutUsRepository)
        {
            _logger = logger;
            _aboutUsRepository = aboutUsRepository;
        }

        /// <summary>
        /// Retrieves the overall review details for products.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves the overall review details for products available in the system.
        /// </remarks>
        /// <returns>Returns a response containing the overall review details.</returns>
        /// <response code="200">Returns a successful response along with the overall review details.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     GET /api/aboutUs/get-overall-review
        /// 
        /// </example>
        [HttpGet("get-overall-review")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<OverAllReview>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetOverAllReview()
        {
            try
            {
                _logger.LogInformation("Fetching details for overall products review starts in controller action method GetOverAllReview \n");

                var reviewDetails = await _aboutUsRepository.GetOverAllReviewAsync();
                if (reviewDetails != null)
                {
                    _logger.LogInformation("Fetching details for overall products review succeeded in controller action method GetOverAllReview \n");
                    return StatusCode(200, new ApiResponse<OverAllReview>(200, "Over all review details..", reviewDetails));
                }
                else
                {
                    _logger.LogError("Fetching details for overall products review failed no review found in controller action method GetOverAllReview \n");
                    return StatusCode(200, new ApiResponse<OverAllReview>(200, "No review found..", null));
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during overall products review fetching in controller action method GetOverAllReview : {sqlEx.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during overall products review fetching in controller action method GetOverAllReview : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
        }
    }
}
