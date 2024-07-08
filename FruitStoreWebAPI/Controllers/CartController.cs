using CommonHelperUtility;
using FruitStoreModels.Cart;
using FruitStoreModels.Response;
using FruitStoreRepositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FruitStoreWebAPI.Controllers
{
    /// <summary>
    /// Represents a class that handles cart related operations.
    /// </summary>
    
    [Route("api/[controller]")]
    [ApiController]
   
    public class CartController : ControllerBase
    {
        private readonly ICartDetailsRepository _cartRepository;
        private readonly ILogger<CartController> _logger;

        /// <summary>
        /// Represents a class that handles cart related operations.
        /// </summary>
        public CartController(ILogger<CartController> logger, ICartDetailsRepository cartRepository)
        {
            _logger = logger;
            _cartRepository = cartRepository;
        }

        /// <summary>
        /// Retrieves details for all products with total in the cart by user Id.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves details with total for all products currently in the cart by user Id.
        /// </remarks>
        /// <param name="userId">The ID of the user whose cart details are to be retrieved.</param>
        /// <returns>Returns a response containing details for all products in the cart.</returns>
        /// <response code="200">Returns a successful response along with product details.</response>
        /// <response code="400">Returns a Bad Request response if the provided user id is invalid</response>
        /// <response code="404">Returns a Not Found response if no products are found in the cart.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     GET /api/cart/get-products-cart-detail/{userId}
        /// 
        /// Sample response (200 - OK):
        /// 
        ///     {
        ///         "statusCode": 200,
        ///         "message": "Cart details..",
        ///         "data": [
        ///             {
        ///                 "cartId": 1,
        ///                 "productImage": "Product 1",
        ///                 "productName": 2,
        ///                 "price": 10.99,
        ///                 "quantity": 1,
        ///                 "userId":1
        ///             },
        ///             {
        ///                 "subTotal": 2,
        ///                 "charges": "Product 2",
        ///                 "total": 1
        ///              
        ///             }
        ///         ]
        ///     }
        /// </example>
        [CustomAuthorizeAttribute("user")]
        [HttpGet("get-products-cart-detail/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<CartDetails>>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetProductsCartDetails(int userId)
        {
            _logger.LogInformation("Validation started in controller action method GetProductsCartDetails \n");

            if (userId == 0)
            {
                _logger.LogError("userId is required  \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid user details..please check and try again."));
            }

            _logger.LogInformation("Validation passed in controller action method GetProductsCartDetails \n");
            try
            {
                _logger.LogInformation($"Fetching details for all products cart details with total starts in controller action method GetProductsCartDetails for user Id :{userId} \n");

                var productsCartWithTotalDetails = await _cartRepository.GetProductsCartDetailsByUserIdAsync(userId);

                var response = new CartWithTotalDetails
                {
                    CartDetails = productsCartWithTotalDetails.Item1,
                    PriceDetails = productsCartWithTotalDetails.Item2
                };

                if ((productsCartWithTotalDetails.Item1 != null) && (productsCartWithTotalDetails.Item2 != null))
                {
                    _logger.LogInformation($"Fetching details for all products cart details with total succeeded in controller action method GetProductsCartDetails for user Id :{userId} \n");
                    return StatusCode(200, new ApiResponse<CartWithTotalDetails>(200, "Cart details..", response));
                }
                else
                {
                    _logger.LogInformation($"Fetching details for all products cart details with total failed no products cart item availabe in controller action method GetProductsCartDetails for user Id :{userId} \n");
                    return StatusCode(200, new ApiResponse<CartWithTotalDetails>(200, "No details available", null));
                }

            }
            catch (SqlException sqlEx)

            {
                if (sqlEx.Message.Contains("User Id required."))
                {
                    _logger.LogError($"Fetching details for all products cart details failed in controller action method GetProductsCartDetails for user Id :{userId} : {sqlEx.Message} \n");

                    return StatusCode(400, new ApiResponse<string>(400, "User Id required."));
                }
                if (sqlEx.Message.Contains("Invalid User Id. User with such User Id does not exist."))
                {
                    _logger.LogError($"Fetching details for all products cart details failed in controller action method GetProductsCartDetails for user Id :{userId} : {sqlEx.Message} \n");

                    return StatusCode(404, new ApiResponse<string>(404, "Invalid user..Please check & try again"));
                }
                _logger.LogError($"SQL Error during fetching for all products cart details with total in controller action method GetProductsCartDetails for user Id :{userId} : {sqlEx.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during fetching for all products cart details with total in controller action method GetProductsCartDetails for user Id :{userId} : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }

        /// <summary>
        /// Retrieves details for the bag counter associated with a user's cart.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves details for the bag counter associated with the specified user's cart.
        /// </remarks>
        /// <param name="userId">The ID of the user whose bag counter details are to be retrieved.</param>
        /// <returns>Returns a response containing details for the bag counter.</returns>
        /// <response code="200">Returns a successful response along with bag counter details.</response>
        /// <response code="404">Returns a not found response.</response>
        /// <response code="400">Returns a Bad Request response if the provided user id is invalid</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     GET /api/cart/get-bag-counter-details?userId=123
        /// 
        /// Sample response (200 - OK):
        /// 
        ///     {
        ///         "statusCode": 200,
        ///         "message": "Bag counter details..",
        ///         "data": {
        ///             "userId": 123,
        ///             "totalItems": 5,
        ///             "totalPrice": 50.99
        ///         }
        ///     }
        ///
        /// </example>
        [AllowAnonymous]
        [HttpGet("get-bag-counter-details/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BagCounterDetails>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetBagCounterDetails(int userId)
        {
            _logger.LogInformation("Validation started in controller action method GetBagCounterDetails \n");

            if (userId == 0)
            {
                _logger.LogError("userId is required  \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid user details..please check and try again."));
            }
            _logger.LogInformation("Validation passed in controller action method GetBagCounterDetails \n");

            try
            {
                _logger.LogInformation("Fetching details for cart bag counter details starts in controller action method GetBagCounterDetails \n");

                var bagCounterDetails = await _cartRepository.GetBagCounterDetailsByUserIdAsync(userId);

                if (bagCounterDetails != null)
                {
                    _logger.LogInformation("Fetching details for cart bag counter details succeeded in controller action method GetBagCounterDetails \n");
                    return StatusCode(200, new ApiResponse<BagCounterDetails>(200, "Bag counter details..", bagCounterDetails));
                }
                else
                {
                    _logger.LogError("Fetching details for cart bag counter details not found in controller action method GetBagCounterDetails \n");
                    return StatusCode(200, new ApiResponse<BagCounterDetails>(200, "No counter details..", null));
                }

            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Message.Contains("User Id required."))
                {
                    _logger.LogError($"Fetching details for cart bag counter details failed in controller action method GetProductsCartDetails for user Id :{userId} : {sqlEx.Message} \n");

                    return StatusCode(400, new ApiResponse<string>(400, "User Id required."));
                }
                if (sqlEx.Message.Contains("Invalid User Id. User with such User Id does not exist."))
                {
                    _logger.LogError($"Fetching details for cart bag counter details failed in controller action method GetProductsCartDetails for user Id :{userId} : {sqlEx.Message} \n");

                    return StatusCode(404, new ApiResponse<string>(404, "Invalid user..Please check & try again"));
                }
                _logger.LogError($"SQL Error during fetching cart bag counter details in controller action method GetBagCounterDetails : {sqlEx.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during fetching cart bag counter details in controller action method GetBagCounterDetails : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }

        /// <summary>
        /// Deletes a product from the cart by its Id.
        /// </summary>
        /// <param name="cartId">The Id of the cart item to delete.</param>
        /// <returns>Returns a response indicating the result of the deletion process.</returns>
        /// <response code="204">Returns a No Content response if the product is successfully deleted.</response>
        /// <response code="400">Returns a Bad Request response if the provided cart Id is invalid or missing.</response>
        /// <response code="404">Returns a Not Found response if no cart item with the provided Id is found.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during the deletion process.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     DELETE /api/cart/delete-products/{cartId}
        /// 
        /// Sample response (204 - No Content):
        /// 
        ///     No content
        /// 
        /// </example>
        [CustomAuthorizeAttribute("user")]
        [HttpDelete("delete-products/{cartId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> DeleteCartItemById(int cartId)
        {
            _logger.LogInformation("Validation started in controller action method DeleteCartItemById \n");

            if (cartId == 0)
            {
                _logger.LogError("CartId is required  \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid cart details..please check and try again."));
            }

            _logger.LogInformation("Validation completed in controller action method DeleteCartItemById \n");

            try
            {
                _logger.LogInformation($"Deleting products cart item starts in controller action method DeleteCartItemById for cartId : {cartId} \n");

                int deletedCartItem = await _cartRepository.DeleteCartItemByIdAsync(cartId);

                if (deletedCartItem == 1)
                {
                    _logger.LogInformation($"Cart item deleted Successfully in controller action method DeleteCartItemById for cartId : {cartId} \n");

                    return StatusCode(204, null);

                }
                else
                {
                    _logger.LogError($"Problem for deletion in controller action method DeleteCartItemById for cartId : {cartId} \n");
                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                }

            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("Cart Id parameter is required."))
                    {
                        _logger.LogError($"Cart item deletion failed in controller action method DeleteCartItemById for cartId : {cartId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid cart data please check and try again."));
                    }
                    else if (sqlException.Message.Contains("Invalid Card Id. Cart with such cart Id does not exist."))
                    {
                        _logger.LogError($"Cart item not found in controller action method DeleteCartItemById for cartId : {cartId} : {sqlException.Message} \n");
                        return StatusCode(404, new ApiResponse<string>(404, "Cart item not exists"));
                    }
                }

                _logger.LogError($"SQL error during cart item deletion in controller action method DeleteCartItemById for cartId : {cartId} : {sqlException.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during cart item deletion in controller action method DeleteCartItemById for cartId : {cartId} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }

        /// <summary>
        /// Creates product cart details in the system.
        /// </summary>
        /// <remarks>
        /// This endpoint allows adding new product cart details in the system.
        /// </remarks>
        /// <param name="cartDetails">The details of the cart to be inserted.</param>
        /// <returns>Returns a response indicating the success or failure of the cart insertion</returns>
        /// <response code="201">Returns a Created response if the cart details are inserted successfully.</response>
        /// <response code="400">Returns a Bad Request response if the provided cart details are invalid or missing.</response>
        /// <response code="404">Returns a Not found response if the provided cart details product name are not found.</response>
        /// <response code="409">Returns a Conflict response if the product name already exists.</response> // Added 409 Conflict
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during insertion or update.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     POST /api//create-cart-details
        ///     {
        ///      
        ///         "ProductName": "Product A",
        ///         "Price": 10.99,
        ///         "Quantity": 100,
        ///         "ProductImage": "string",
        ///         "UserId": "1",
        ///      
        ///     }
        /// </example>
        [CustomAuthorizeAttribute("user")]
        [HttpPost("create-cart-details")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> CreateCartDetails(AddCartDetails cartDetails)
        {
            _logger.LogInformation("Validation started in controller action method CreateCartDetails \n");

            _logger.LogInformation("ProductName validation started \n");

            if (string.IsNullOrEmpty(cartDetails.ProductName))
            {
                _logger.LogError("ProductName is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "ProductName is required."));
            }
            else if (cartDetails.ProductName.Length > 255)
            {
                _logger.LogError("ProductName must be at most 255 characters long \n");

                return StatusCode(400, new ApiResponse<string>(400, "ProductName must be at most 255 characters long."));
            }

            _logger.LogInformation("ProductName validation passed \n");

            _logger.LogInformation("Price validation started \n");

            if (cartDetails.Price == 0)
            {
                _logger.LogError("Price can not be null \n");

                return StatusCode(400, new ApiResponse<string>(400, "Price can not be null"));
            }
            else if (cartDetails.Price < 0 || cartDetails.Price > 999999.99m)
            {
                _logger.LogError("Price must be between 0.01 and 999999.99 \n");

                return StatusCode(400, new ApiResponse<string>(400, "Price must be between 1 and 999999.99."));
            }

            _logger.LogInformation("Price validation passed \n");

            _logger.LogInformation("UserId validation started \n");

            if (cartDetails.UserId == 0)
            {
                _logger.LogError("UserId is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "Invalid User..try again"));
            }
            _logger.LogInformation("UserId validation passed \n");

            _logger.LogInformation("Subtotal validation started \n");

            if (cartDetails.SubTotal == 0)
            {
                _logger.LogError("Subtotal can not be null \n");

                return StatusCode(400, new ApiResponse<string>(400, "Subtotal can not be null"));
            }
            else if (cartDetails.SubTotal < 0)
            {
                _logger.LogError("SubTotal must be positive value. \n");

                return StatusCode(400, new ApiResponse<string>(400, "SubTotal must be positive value."));
            }

            _logger.LogInformation("Subtotal validation passed \n");

            _logger.LogInformation("CurrentQuant validation started \n");

            if (cartDetails.CurrentQuant == 0)
            {
                _logger.LogError("CurrentQuant can not be null \n");

                return StatusCode(400, new ApiResponse<string>(400, "CurrentQuant can not be null"));
            }
            else if (cartDetails.SubTotal < 0)
            {
                _logger.LogError("CurrentQuant must be positive value. \n");

                return StatusCode(400, new ApiResponse<string>(400, "CurrentQuant must be positive value."));
            }

            _logger.LogInformation("CurrentQuant validation passed \n");

            _logger.LogInformation("Validation completed in controller action method CreateCartDetails \n");

            try
            {
                _logger.LogInformation($"Creating products cart details starts in controller action method CreateCartDetails for product name : {cartDetails.ProductName} \n");

                int insertStatus = await _cartRepository.InsertCartDetailsAsync(cartDetails);

                if (insertStatus != 0)
                {
                    _logger.LogInformation($"Creating products cart details completed in controller action method CreateCartDetails for product name : {cartDetails.ProductName} \n");

                    return StatusCode(201, new ApiResponse<string>(201, "Item added to cart.."));
                }
                else
                {
                    _logger.LogError($"Creating products cart details  failed unexpectedly in controller action method CreateCartDetails for product name : {cartDetails.ProductName} \n");

                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("ProductName cannot be NULL or empty."))
                    {
                        _logger.LogError($"Creating cart details failed in controller action method CreateCartDetails for product name : {cartDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "ProductName cannot be NULL or empty."));
                    }
                    else if (sqlException.Message.Contains("Price must be a positive value."))
                    {
                        _logger.LogError($"Creating cart details failed in controller action method  CreateCartDetails for product name : {cartDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Price must be a positive value."));
                    }
                    else if (sqlException.Message.Contains("Product image required."))
                    {
                        _logger.LogError($"Creating cart details failed in controller action method  CreateCartDetails for product name : {cartDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Product image required."));
                    }
                    else if (sqlException.Message.Contains("User details required."))
                    {
                        _logger.LogError($"Creating cart details failed in controller action method  CreateCartDetails for product name : {cartDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "User details required"));
                    }
                    else if (sqlException.Message.Contains("Product name not exists."))
                    {
                        _logger.LogError($"Creating cart details failed in controller action method  CreateCartDetails for product name : {cartDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(404, new ApiResponse<string>(404, "Product name not exists."));
                    }
                    else if (sqlException.Message.Contains("Product already exists."))
                    {
                        _logger.LogError($"Creating cart details failed in controller action method  CreateCartDetails for product name : {cartDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(409, new ApiResponse<string>(409, "Hey there! Looks like this item is already in your cart. Adjust quantity as needed."));
                    }
                    else if (sqlException.Message.Contains("User not exists."))
                    {
                        _logger.LogError($"Creating cart details failed in controller action method  CreateCartDetails for product name : {cartDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(404, new ApiResponse<string>(404, "Invalid user..login & try again"));
                    }
                    else if (sqlException.Message.Contains("Subtotal must be a positive value."))
                    {
                        _logger.LogError($"Creating cart details failed in controller action method  CreateCartDetails for product name : {cartDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Subtotal must be a positive value"));
                    }
                }
                _logger.LogError($"SQL Error during card create in controller action method CreateCartDetails for product name : {cartDetails.ProductName} : {sqlException.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during card create in controller action method CreateCartDetails for product name : {cartDetails.ProductName} : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }

        /// <summary>
        /// Updates product cart details in the system.
        /// </summary>
        /// <remarks>
        /// This endpoint allows updating existing product cart details in the system.
        /// </remarks>
        /// <param name="cartDetails">The details of the cart to be updated.</param>
        /// <returns>Returns a response indicating the success or failure of the cart update.</returns>
        /// <response code="200">Returns a OK response if the cart details are updated successfully.</response>
        /// <response code="400">Returns a Bad Request response if the provided cart details are invalid or missing.</response>
        /// <response code="404">Returns a Not Found response if the provided cart details are not found.</response>
        /// <response code="409">Returns a Conflict response if there are conflicts with the provided cart details.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during the update.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     PATCH /api/update-cart-details
        ///     {
        ///         "CartId": 1,
        ///         "Price": 10.99,
        ///         "Quantity": 100,
        ///         "UserId": 1,
        ///         "SubTotal": 1099.00
        ///     }
        /// </example>
        [CustomAuthorizeAttribute("user")]
        [HttpPatch("update-cart-details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> UpdateCartDetails(UpdateCartDetails cartDetails)
        {
            _logger.LogInformation("Validation started in controller action method UpdateCartDetails \n");

            _logger.LogInformation("CartId validation started \n");

            if (cartDetails.CartId == 0)
            {
                _logger.LogError("Cart Id can not be null \n");

                return StatusCode(400, new ApiResponse<string>(400, "Invalid cart details"));
            }
            _logger.LogInformation("CartId validation passed \n");

            _logger.LogInformation("Price validation started \n");

            if (cartDetails.Price == 0)
            {
                _logger.LogError("Price can not be null \n");

                return StatusCode(400, new ApiResponse<string>(400, "Price can not be null"));
            }
            else if (cartDetails.Price < 0 || cartDetails.Price > 999999.99m)
            {
                _logger.LogError("Price must be between 0.01 and 999999.99 \n");

                return StatusCode(400, new ApiResponse<string>(400, "Price must be between 1 and 999999.99."));
            }

            _logger.LogInformation("Price validation passed \n");

            _logger.LogInformation("Quantity validation started \n");

            if (cartDetails.Quantity == 0)
            {
                _logger.LogError("Quantity is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "Quantity is required."));
            }
            else if (cartDetails.Quantity < 1 || cartDetails.Quantity > 10000)
            {
                _logger.LogError("Quantity must be between 1 and 10000 \n");

                return StatusCode(400, new ApiResponse<string>(400, "Quantity must be between 1 and 10000"));
            }
            _logger.LogInformation("Quantity validation passed \n");

            _logger.LogInformation("UserId validation started \n");

            if (cartDetails.UserId == 0)
            {
                _logger.LogError("UserId is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "Invalid User..try again"));
            }
            _logger.LogInformation("UserId validation passed \n");

            _logger.LogInformation("Subtotal validation started \n");

            if (cartDetails.SubTotal == 0)
            {
                _logger.LogError("Subtotal can not be null \n");

                return StatusCode(400, new ApiResponse<string>(400, "Subtotal can not be null"));
            }
            else if (cartDetails.SubTotal < 0)
            {
                _logger.LogError("SubTotal must be positive value. \n");

                return StatusCode(400, new ApiResponse<string>(400, "SubTotal must be positive value."));
            }

            _logger.LogInformation("Subtotal validation passed \n");

            _logger.LogInformation("TotalQuant validation started \n");

            if (cartDetails.TotalQuant == 0)
            {
                _logger.LogError("TotalQuant can not be null \n");

                return StatusCode(400, new ApiResponse<string>(400, "TotalQuant can not be null"));
            }
            else if (cartDetails.SubTotal < 0)
            {
                _logger.LogError("TotalQuant must be positive value. \n");

                return StatusCode(400, new ApiResponse<string>(400, "TotalQuant must be positive value."));
            }

            _logger.LogInformation("TotalQuant validation passed \n");

            _logger.LogInformation("Validation completed in controller action method UpdateCartDetails \n");

            try
            {
                _logger.LogInformation($"Updating products cart details starts in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} \n");

                int updateStatus = await _cartRepository.UpdateCartDetailsAsync(cartDetails);

                if (updateStatus != 0)
                {
                    _logger.LogInformation($"Updating products cart details completed in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} \n");

                    return StatusCode(200, new ApiResponse<string>(200, "Item updated successfully.."));
                }
                else
                {
                    _logger.LogError($"Updating products cart details  failed unexpectedly in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} \n");

                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("CartId cannot be NULL or empty."))
                    {
                        _logger.LogError($"Updating cart details failed in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid cart details."));
                    }
                    else if (sqlException.Message.Contains("Price must be a positive value."))
                    {
                        _logger.LogError($"Updating cart details failed in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Price must be a positive value."));
                    }
                    else if (sqlException.Message.Contains("Quantity must be a positive value."))
                    {
                        _logger.LogError($"Updating cart details failed in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Quantity must be a positive value."));
                    }
                    else if (sqlException.Message.Contains("User details required."))
                    {
                        _logger.LogError($"Updating cart details failed in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "User details required"));
                    }
                    else if (sqlException.Message.Contains("User not exists."))
                    {
                        _logger.LogError($"Updating cart details failed in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} : {sqlException.Message} \n");

                        return StatusCode(404, new ApiResponse<string>(404, "User not exists..login again"));
                    }
                    else if (sqlException.Message.Contains("Subtotal must be a positive value."))
                    {
                        _logger.LogError($"Updating cart details failed in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Subtotal must be a positive value"));
                    }
                    else if (sqlException.Message.Contains("Current item is out of stock."))
                    {
                        _logger.LogError($"Updating cart details failed in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Current item is out of stock."));
                    }
                    else if (sqlException.Message.Contains("Cart details not exists."))
                    {
                        _logger.LogError($"Updating cart details failed in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} : {sqlException.Message} \n");

                        return StatusCode(404, new ApiResponse<string>(404, "Cart details not exists."));
                    }
                }
                _logger.LogError($"SQL Error during cart update in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} : {sqlException.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during card update in controller action method UpdateCartDetails for cart Id : {cartDetails.CartId} : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }

    }
}
