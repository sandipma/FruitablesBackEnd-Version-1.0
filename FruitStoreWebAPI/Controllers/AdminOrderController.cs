using CommonHelperUtility;
using FruitStoreModels.Admin_Orders;
using FruitStoreModels.Response;
using FruitStoreRepositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FruitStoreWebAPI.Controllers
{
    /// <summary>
    /// Represents a class that handles admin orders related operations.
    /// </summary>
    [CustomAuthorizeAttribute("admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminOrderController : ControllerBase
    {
        private readonly ILogger<AdminOrderController> _logger;
        private readonly IAdminOrderRepository _orderRepository;

        /// <summary>
        /// Represents a class that handles admin orders screen operations.
        /// </summary>
        public AdminOrderController(ILogger<AdminOrderController> logger, IAdminOrderRepository orderRepository)
        {
            _logger = logger;
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Retrieves details for all orders.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves details for all orders.
        /// </remarks>
        /// <returns>Returns a response containing details for all orders.</returns>
        /// <response code="200">Returns a successful response along with order details.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     GET /api/adminorder/get-all-orders
        /// 
        /// Sample response (200 - OK):
        /// 
        ///     {
        ///         "statusCode": 200,
        ///         "message": "Order details..",
        ///         "data": [
        ///             {
        ///                 "orderId": 1,
        ///                 "orderDate": "2024-05-15T10:30:00",
        ///                 "userId": 1,
        ///                 "totalAmount": 100.99,
        ///                 "status": "Completed"
        ///             },
        ///             {
        ///                 "orderId": 2,
        ///                 "orderDate": "2024-05-15T11:00:00",
        ///                 "userId": 2,
        ///                 "totalAmount": 50.49,
        ///                 "status": "Pending"
        ///             }
        ///         ]
        ///     }
        /// </example>
        [HttpGet("get-all-orders")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<AdminOrder>>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                _logger.LogInformation("Fetching details for orders starts in controller action method GetAllOrders \n");

                var orderData = await _orderRepository.GetAllOrdersAsync();

                if (orderData != null)
                {
                    _logger.LogInformation("Fetching details for orders successfully completed in controller action method GetAllOrders \n");
                    return StatusCode(200, new ApiResponse<List<AdminOrder>>(200, "Order details..", orderData.ToList()));
                }
                else
                {
                    _logger.LogInformation("Fetching details for orders failed no orders availabe in controller action method GetAllOrders \n");
                    return StatusCode(200, new ApiResponse<List<AdminOrder>>(200, "No order available", null));
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during order fetching in controller action method GetAllOrders : {sqlEx.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during orders fetching in controller action method GetAllOrders : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }
    }
}
