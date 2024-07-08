using CommonHelperUtility;
using FruitStoreModels.Order;
using FruitStoreModels.Payment;
using FruitStoreModels.Response;
using FruitStoreRepositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FruitStoreWebAPI.Controllers
{
    /// <summary>
    /// Represents a class that handles order related operations.
    /// </summary>
    [CustomAuthorizeAttribute("user")]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;

        /// <summary>
        /// Represents a class that handles order related operations.
        /// </summary>
        public OrderController(ILogger<OrderController> logger, IPaymentRepository paymentRepository, IOrderRepository orderRepository)
        {
            _logger = logger;
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Creates order details for a user.
        /// </summary>
        /// <remarks>
        /// This endpoint creates order details for a user by validating the provided order details, including product information, subtotal, charges, total, user ID, and mode of payment.
        /// </remarks>
        /// <param name="orderDetails">The object containing order details to be created.</param>
        /// <returns>Returns a response indicating the status of the order creation process.</returns>
        /// <response code="201">Returns a successful response if the order is created successfully.</response>
        /// <response code="400">Returns a Bad Request response if any of the provided details are invalid or missing.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during order creation.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     POST /api/order/create-order-details
        ///     {
        ///         "cartDetails": [
        ///             {
        ///                 "productImage": "image_url",
        ///                 "productName": "Product 1",
        ///                 "price": 10.99,
        ///                 "quantity": 2
        ///             },
        ///             {
        ///                 "productImage": "image_url",
        ///                 "productName": "Product 2",
        ///                 "price": 29.99,
        ///                 "quantity": 1
        ///             }
        ///         ],
        ///         "subTotal": 50.97,
        ///         "charges": 2.00,
        ///         "total": 52.97,
        ///         "userId": 1,
        ///         "modeOfPayment": "Credit Card"
        ///     }
        /// 
        /// Sample response (201 - Created):
        /// 
        ///     {
        ///         "statusCode": 201,
        ///         "message": "Order created successfully.",
        ///         "data": {
        ///             "orderId": 123,
        ///             "orderDate": "2024-05-13T10:00:00",
        ///             "subTotal": 50.97,
        ///             "charges": 2.00,
        ///             "total": 52.97
        ///         }
        ///     }
        /// </example>
        [HttpPost("create-order-details")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<OrderDetails>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> CreateOrderDetails(AddOrderDetails orderDetails)
        {
            _logger.LogInformation("Validation started in controller action method CreateOrderDetails \n");

            foreach (OrderCartDetails details in orderDetails.cartDetails)
            {
                _logger.LogInformation("ProductImage validation started  \n");

                if (string.IsNullOrEmpty(details.ProductImage))
                {
                    _logger.LogError("ProductImage is required \n");
                    return StatusCode(400, new ApiResponse<string>(400, "Product detail is required."));
                }
                _logger.LogInformation("ProductImage validation passed  \n");

                _logger.LogInformation("ProductName validation started  \n");
                if (string.IsNullOrEmpty(details.ProductName))
                {
                    _logger.LogError("ProductName is required \n");
                    return StatusCode(400, new ApiResponse<string>(400, "Product detail is required."));
                }
                else if (details.ProductName.Length > 255)
                {
                    _logger.LogError("ProductName must be at most 255 characters long \n");
                    return StatusCode(400, new ApiResponse<string>(400, "ProductName must be at most 255 characters long."));
                }
                _logger.LogInformation("ProductName validation passed  \n");

                _logger.LogInformation("Price validation started \n");
                if (details.Price == 0)
                {
                    _logger.LogError("Price can not be null \n");
                    return StatusCode(400, new ApiResponse<string>(400, "Price can not be null"));
                }
                else if (details.Price < 0 || details.Price > 999999.99m)
                {
                    _logger.LogError("Price must be between 0.01 and 999999.99 \n");

                    return StatusCode(400, new ApiResponse<string>(400, "Price must be between 1 and 999999.99."));
                }
                _logger.LogInformation("Price validation passed \n");

                _logger.LogInformation("Quantity validation started \n");
                if (details.Quantity == 0)
                {
                    _logger.LogError("Quantity can not be null \n");
                    return StatusCode(400, new ApiResponse<string>(400, "Quantity can not be null"));
                }
                else if (details.Quantity < 0)
                {
                    _logger.LogError("Quantity must be a non-negative value \n");
                    return StatusCode(400, new ApiResponse<string>(400, "Quantity must be a non-negative value"));
                }
                _logger.LogInformation("Quantity validation passed \n");
            }
            _logger.LogInformation("SubTotal validation started \n");

            if (orderDetails.SubTotal == 0)
            {
                _logger.LogError("Subtotal can not be null \n");
                return StatusCode(400, new ApiResponse<string>(400, "Subtotal can not be null"));
            }
            else if (orderDetails.SubTotal < 0)
            {
                _logger.LogError("SubTotal must be positive value \n");
                return StatusCode(400, new ApiResponse<string>(400, "SubTotal must be positive value."));
            }
            _logger.LogInformation("SubTotal validation passed \n");

            _logger.LogInformation("Charges validation started \n");

            if (orderDetails.Charges == 0)
            {
                _logger.LogError("Charges can not be null \n");
                return StatusCode(400, new ApiResponse<string>(400, "Charges can not be null"));
            }
            else if (orderDetails.Charges < 0)
            {
                _logger.LogError("Charges must be a non-negative value \n");
                return StatusCode(400, new ApiResponse<string>(400, "Charges must be a non-negative value"));
            }
            _logger.LogInformation("Charges validation passed \n");

            _logger.LogInformation("Total validation started \n");

            if (orderDetails.Total == 0)
            {
                _logger.LogError("Total can not be null \n");
                return StatusCode(400, new ApiResponse<string>(400, "Total can not be null"));
            }
            else if (orderDetails.Total < 0)
            {
                _logger.LogError("Total must be a non-negative value \n");
                return StatusCode(400, new ApiResponse<string>(400, "Total must be a non-negative value"));
            }
            _logger.LogInformation("Total validation started \n");

            _logger.LogInformation("UserId validation started \n");

            if (orderDetails.UserId == 0)
            {
                _logger.LogError("UserId is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid User..try again"));
            }

            _logger.LogInformation("UserId validation passed \n");

            _logger.LogInformation("ModeOfPayment validation started \n");

            if (string.IsNullOrEmpty(orderDetails.ModeOfPayment))
            {
                _logger.LogError("Mode of payment is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Kindly select mode of payment."));
            }
            _logger.LogInformation("ModeOfPayment validation passed \n");


            _logger.LogInformation("Validation passed in controller action method CreateOrderDetails");

            try
            {
                _logger.LogInformation($"Processing order for user Id : {orderDetails.UserId} \n");

                OrderDetails completeOrderDetails = await _paymentRepository.ProcessOrderAsync(orderDetails);

                if (completeOrderDetails != null)
                {
                    _logger.LogInformation($"Order created successfully in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} \n");
                    return StatusCode(201, new ApiResponse<OrderDetails>(201, "Order created successfully.", completeOrderDetails));
                }
                _logger.LogError($"Order failed unexpectedly in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("Product image required."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid product data"));
                    }
                    else if (sqlException.Message.Contains("ProductName cannot be NULL or empty."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "ProductName cannot be NULL or empty."));
                    }
                    else if (sqlException.Message.Contains("Price must be a positive value."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Price must be a positive value."));
                    }
                    else if (sqlException.Message.Contains("Quantity must be a non-negative value."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Quantity must be a non-negative value."));
                    }
                    else if (sqlException.Message.Contains("Subtotal must be a positive value."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Subtotal must be a positive value."));
                    }
                    else if (sqlException.Message.Contains("Charges must be a positive value."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Charges must be a positive value."));
                    }
                    else if (sqlException.Message.Contains("Total must be a positive value."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Total must be a positive value."));
                    }
                    else if (sqlException.Message.Contains("User Id required."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid user"));
                    }
                    else if (sqlException.Message.Contains("RazorPayOrderId is required."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid user"));
                    }
                    else if (sqlException.Message.Contains("Order status required."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid order"));
                    }
                    else if (sqlException.Message.Contains("Mode of payment cannot be NULL or empty."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Kindly select mode of payment"));
                    }
                    else if (sqlException.Message.Contains("Amount is required."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid amount"));
                    }
                    else if (sqlException.Message.Contains("Amount due is not filled."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid amount"));
                    }
                    else if (sqlException.Message.Contains("Currency is required."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid order details"));
                    }
                    else if (sqlException.Message.Contains("Receipt is required."))
                    {
                        _logger.LogError($"Creating order details failed in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid order details"));
                    }
                }
                _logger.LogError($"SQL Error during order create in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {sqlException.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during order create in controller action method CreateOrderDetails for user Id : {orderDetails.UserId} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
        }

        /// <summary>
        /// Completes the order processing for a user.
        /// </summary>
        /// <remarks>
        /// This endpoint completes the order processing for a user by validating the order details, payment information, and user details.
        /// </remarks>
        /// <param name="completeOrderProcess">The object containing order processing details.</param>
        /// <returns>Returns a response indicating the status of the order processing.</returns>
        /// <response code="200">Returns a successful response if the order processing is completed successfully.</response>
        /// <response code="400">Returns a Bad Request response if any of the provided details are invalid or missing.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during order processing.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     POST /api/order/complete-order-process
        ///     {
        ///         "orderId": 1,
        ///         "razorpayOrderId": "order123",
        ///         "paymentId": "payment123",
        ///         "signature": "signature123",
        ///         "modeOfPayment": "Credit Card",
        ///         "userId": 1
        ///     }
        /// 
        /// Sample response (200 - OK):
        /// 
        ///     {
        ///         "statusCode": 200,
        ///         "message": "Order and payment done successfully."
        ///     }
        /// </example>
        [HttpPost("complete-order-process")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<OrderDetails>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> CompleteOrderProcess(CompleteOrderProcess completeOrderProcess)
        {
            _logger.LogInformation("Validation started in controller action method CompleteOrderProcess \n");

            _logger.LogInformation("OrderId validation started \n");

            if (completeOrderProcess.OrderId == 0)
            {
                _logger.LogError("Invalid order details \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid order details"));
            }
            _logger.LogInformation("OrderId validation passed \n");

            _logger.LogInformation("Razorpay order Id validation started \n");

            if (string.IsNullOrEmpty(completeOrderProcess.RazorpayOrderId))
            {
                _logger.LogError("Invalid order details \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid order details"));
            }
            _logger.LogInformation("Razorpay order Id validation passed \n");

            _logger.LogInformation("Payment Id validation started \n");

            if (string.IsNullOrEmpty(completeOrderProcess.PaymentId))
            {
                _logger.LogError("Invalid payment details \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid payment details"));
            }
            _logger.LogInformation("Payment Id validation passed \n");

            _logger.LogInformation("Signature validation started \n");

            if (string.IsNullOrEmpty(completeOrderProcess.Signature))
            {
                _logger.LogError("Invalid payment details \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid payment details"));
            }
            _logger.LogInformation("Signature validation passed \n");

            _logger.LogInformation("ModeOfPayment validation started \n");

            if (string.IsNullOrEmpty(completeOrderProcess.ModeOfPayment))
            {
                _logger.LogError("Invalid mode of payment \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid mode of payment"));
            }
            _logger.LogInformation("ModeOfPayment validation passed \n");

            _logger.LogInformation("UserId validation started \n");

            if (completeOrderProcess.UserId == 0)
            {
                _logger.LogError("Invalid user details \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid user details"));
            }
            _logger.LogInformation("UserId validation passed \n");

            _logger.LogInformation("Validation passed in controller action method CompleteOrderProcess \n");
            try
            {
                _logger.LogInformation($"Processing order and payment in controller action method CompleteOrderProcess for user Id : {completeOrderProcess.UserId} \n");

                int completeOrderDetails = await _paymentRepository.CompleteOrderProcessAsync(completeOrderProcess);

                if (completeOrderDetails != 0)
                {
                    _logger.LogInformation($"Order processing and payment done successfully in controller action method CompleteOrderProcess for user Id : {completeOrderProcess.UserId} \n");
                    return StatusCode(200, new ApiResponse<OrderDetails>(200, "Order and payment done successfully."));
                }
                _logger.LogError($"Order processing and payment unexpectedly failed in controller action method CompleteOrderProcess for user Id : {completeOrderProcess.UserId} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("OrderId cannot be NULL or empty."))
                    {
                        _logger.LogError($"Order processing and payment unexpectedly failed in controller action method CompleteOrderProcess for user Id : {completeOrderProcess.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid order details"));
                    }
                    else if (sqlException.Message.Contains("User details required."))
                    {
                        _logger.LogError($"Order processing and payment unexpectedly failed in controller action method CompleteOrderProcess for user Id : {completeOrderProcess.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "User details required"));
                    }
                    else if (sqlException.Message.Contains("Mode of payment required."))
                    {
                        _logger.LogError($"Order processing and payment unexpectedly failed in controller action method CompleteOrderProcess for user Id : {completeOrderProcess.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Mode of payment required"));
                    }
                    else if (sqlException.Message.Contains("Order status required."))
                    {
                        _logger.LogError($"Order processing and payment unexpectedly failed in controller action method CompleteOrderProcess for user Id : {completeOrderProcess.UserId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid order details"));
                    }
                }
                _logger.LogError($"SQL Error during order processing and payment unexpectedly failed in controller action method CompleteOrderProcess for user Id : {completeOrderProcess.UserId} : {sqlException.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during order processing and payment unexpectedly failed in controller action method CompleteOrderProcess for user Id : {completeOrderProcess.UserId} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
        }

        /// <summary>
        /// Retrieves order details for a specific user by their user Id.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves order details for a user identified by their user Id.
        /// </remarks>
        /// <param name="userId">The ID of the user whose order details are to be retrieved.</param>
        /// <returns>Returns a response containing order details for the specified user.</returns>
        /// <response code="200">Returns a successful response along with order details.</response>
        /// <response code="400">Returns a Bad Request response if the provided user id is invalid.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     GET /api/order/get-order-details/{userId}
        /// 
        /// Sample response (200 - OK):
        /// 
        ///     {
        ///         "statusCode": 200,
        ///         "message": "Order details..",
        ///         "data": {
        ///             "orderId": 1,
        ///             "orderDate": "2024-05-13T10:00:00",
        ///             "totalAmount": 50.99,
        ///             "items": [
        ///                 {
        ///                     "itemId": 1,
        ///                     "itemName": "Product 1",
        ///                     "quantity": 2,
        ///                     "price": 10.99
        ///                 },
        ///                 {
        ///                     "itemId": 2,
        ///                     "itemName": "Product 2",
        ///                     "quantity": 1,
        ///                     "price": 29.99
        ///                 }
        ///             ]
        ///         }
        ///     }
        /// </example>
        [HttpGet("get-order-details/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<UserOrderDetails>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetOrderDetailsByUserId(int userId)
        {
            _logger.LogInformation("Validation started in controller action method GetOrderDetailsByUserId \n");

            if (userId == 0)
            {
                _logger.LogError("userId is required  \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid user details..please check and try again."));
            }

            _logger.LogInformation("Validation passed in controller action method GetOrderDetailsByUserId \n");

            try
            {
                _logger.LogInformation($"Fetching details for order details by user Id in controller action method GetOrderDetailsByUserId for user Id :{userId} \n");

                var orderDetails = await _orderRepository.GetOrderDetailsByUserIdAsync(userId);


                if (orderDetails != null)
                {
                    _logger.LogInformation($"Fetching order details by user Id succeeded in controller action method GetOrderDetailsByUserId for user Id :{userId} \n");
                    return StatusCode(200, new ApiResponse<List<UserOrderDetails>>(200, "Order details..", orderDetails));
                }
                else
                {
                    _logger.LogError($"Fetching order details by user Id failed in controller action method GetOrderDetailsByUserId for user Id :{userId} \n");
                    return StatusCode(200, new ApiResponse<List<UserOrderDetails>>(200, "Order details not found..", null));
                }

            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    if (sqlEx.Message.Contains("User details required."))
                    {
                        _logger.LogError($"Creating new category failed in controller action method GetOrderDetailsByUserId for user Id :{userId} : {sqlEx.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Category cannot be NULL"));
                    }
                }
                _logger.LogError($"SQL Error during fetching order details by user Id in controller action method GetOrderDetailsByUserId for user Id : {userId} : {sqlEx.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during fetching for order details by user Id in controller action method GetOrderDetailsByUserId for user Id : {userId} : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }

        /// <summary>
        /// Updates the status details of an order.
        /// </summary>
        /// <remarks>
        /// This endpoint updates the status details of an order.
        /// </remarks>
        /// <param name="updateStatus">The object containing the updated order status details.</param>
        /// <returns>Returns a response indicating the success or failure of the operation.</returns>
        /// <response code="200">Returns a successful response if the order status is updated successfully.</response>
        /// <response code="400">Returns a Bad Request response if the provided order status or user ID is invalid.</response>
        /// <response code="404">Returns a Not Found response if no orders status are found for the user.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     POST /api/order/update-order-status
        ///     {
        ///         "orderId": 1,
        ///         "userId": 123,
        ///         "orderStatus": "Shipped"
        ///     }
        /// 
        /// Sample response (200 - OK):
        /// 
        ///     {
        ///         "statusCode": 200,
        ///         "message": "Order status updated successfully."
        ///     }
        /// </example>
        [HttpPut("update-order-status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> UpdateOrderStatusDetails(UpdateOrderStatus updateStatus)
        {
            _logger.LogInformation("Validation started in controller action method UpdateOrderStatusDetails \n");

            _logger.LogInformation("OrderStatus validation started \n");

            if (updateStatus.OrderStatus == null)
            {
                _logger.LogError("Order status can not be null \n");

                return StatusCode(400, new ApiResponse<string>(400, "Order status can not be null"));
            }
            _logger.LogInformation("OrderStatus validation passed \n");

            _logger.LogInformation("User Id validation started \n");

            if (updateStatus.UserId <= 0)
            {
                _logger.LogError("User Id can not be null \n");

                return StatusCode(400, new ApiResponse<string>(400, "Invalid user"));
            }
            _logger.LogInformation("User Id validation passed \n");

            _logger.LogInformation("Validation completed in controller action method UpdateOrderStatusDetails \n");

            try
            {
                _logger.LogInformation($"Updating order status details starts in controller action method UpdateOrderStatusDetails for user Id : {updateStatus.UserId} \n");

                int updateStatusDetails = await _orderRepository.UpdateOrderStatusAsync(updateStatus);

                if (updateStatusDetails != 0)
                {
                    _logger.LogInformation($"Updating order status details completed in controller action method UpdateOrderStatusDetails for user Id : {updateStatus.UserId} \n");

                    return StatusCode(200, new ApiResponse<string>(200, "Order status updated successfully.."));
                }
                else
                {
                    _logger.LogError($"Updating order status details failed in controller action method UpdateOrderStatusDetails for user Id : {updateStatus.UserId} \n");

                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("Order Status cannot be NULL or empty."))
                    {
                        _logger.LogError($"Updating order status details failed in controller action method UpdateOrderStatusDetails for user Id : {updateStatus.UserId} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid order"));
                    }
                    else if (sqlException.Message.Contains("UserId cannot be NULL or empty."))
                    {
                        _logger.LogError($"Updating order status details failed in controller action method UpdateOrderStatusDetails for user Id : {updateStatus.UserId} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid user"));
                    }
                    else if (sqlException.Message.Contains("Invalid User Id. User with such User Id does not exist."))
                    {
                        _logger.LogError($"Updating order status details failed in controller action method UpdateOrderStatusDetails for user Id : {updateStatus.UserId} : {sqlException.Message} \n");
                        return StatusCode(404, new ApiResponse<string>(404, "User not exists"));
                    }
                }
                _logger.LogError($"SQL Error during updating order status details in controller action method UpdateOrderStatusDetails for user Id : {updateStatus.UserId} : {sqlException.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during updating order status details in controller action method UpdateOrderStatusDetails for user Id : {updateStatus.UserId} : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }

    }
}
