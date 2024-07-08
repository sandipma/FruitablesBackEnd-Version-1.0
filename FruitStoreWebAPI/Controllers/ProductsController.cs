using CommonHelperUtility;
using ExternalService.Interfaces;
using FruitStoreModels.Products;
using FruitStoreModels.Response;
using FruitStoreRepositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using System.Data.SqlClient;


namespace FruitStoreWebAPI.Controllers
{
    /// <summary>
    /// Represents a class that handles product related operations.
    /// </summary>
    [CustomAuthorizeAttribute("admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {

        private readonly IProductsRepository _productsRepository;
        private readonly ILogger<ProductsController> _logger;

        /// <summary>
        /// Represents a class that handles product related operations.
        /// </summary>
        public ProductsController(ILogger<ProductsController> logger, IProductsRepository productsRepository)
        {
            _logger = logger;
            _productsRepository = productsRepository;
        }

        /// <summary>
        /// Retrieves details for all products.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves details for all products available in the system.
        /// </remarks>
        /// <returns>Returns a response containing details for all products.</returns>
        /// <response code="200">Returns a successful response along with product details.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     GET /api/products/get-all-products
        /// 
        /// </example>
        [HttpGet("get-all-products")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<ProductsDetails>>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                _logger.LogInformation("Fetching details for products starts in controller action method GetAllProducts \n");

                var productDetails = await _productsRepository.GetAllProductsAsync();

                if (productDetails != null)
                {             
                    _logger.LogInformation("Fetching details for products succeeded in controller action method GetAllProducts \n");
                    return StatusCode(200, new ApiResponse<List<ProductsDetails>>(200, "Products details..", productDetails));
                }
                else
                {
                    _logger.LogError("Fetching details for products failed no products availabe in controller action method GetAllProducts \n");
                    return StatusCode(200, new ApiResponse<List<ProductsDetails>>(200, "No products found..", null));
                }

            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during product fetching in controller action method GetAllProducts : {sqlEx.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during product fetching in controller action method GetAllProducts : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }


        /// <summary>
        /// Creates or updates products in the system.
        /// </summary>
        /// <remarks>
        /// This endpoint allows adding new products or updating existing ones in the system.
        /// </remarks>
        /// <param name="addProductsDetails">The details of the products to be inserted or updated.</param>
        /// <returns>Returns a response indicating the success or failure of the product insertion or update.</returns>
        /// <response code="201">Returns a Created response if the products are inserted successfully.</response>
        /// <response code="200">Returns an OK response if the products are updated successfully.</response>
        /// <response code="400">Returns a Bad Request response if the provided product details are invalid or missing.</response>
        /// <response code="409">Returns a conflict if alraedy exits.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during insertion or update.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     POST /api/create-update-products
        ///     {
        ///         "CategoryId": 1,
        ///         "ProductName": "Product A",
        ///         "Price": 10.99,
        ///         "StockQuantity": 100,
        ///         "ImageData": "base64_encoded_image_data",
        ///         "isInsertOrUpdateAction": "I"
        ///     }
        /// </example>
        [HttpPost("create-update-products")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> CreateUpdateProducts([FromForm] AddUpdateProductsDetails addProductsDetails)
        {
            _logger.LogInformation("Validation started in controller action method CreateUpdateProducts \n");


            _logger.LogInformation("CategoryId validation started \n");

            if (addProductsDetails.CategoryId == 0)
            {
                _logger.LogError("CategoryId is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "CategoryId is required."));
            }

            _logger.LogInformation("CategoryId validation passed \n");

            _logger.LogInformation("AdminId validation started \n");

            if (string.IsNullOrEmpty(addProductsDetails.AdminId))
            {
                _logger.LogError("Admin details is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "Admin details is required."));
            }

            _logger.LogInformation("AdminId validation passed \n");

            _logger.LogInformation("ProductName validation started \n");

            if (string.IsNullOrEmpty(addProductsDetails.ProductName))
            {
                _logger.LogError("ProductName is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "ProductName is required."));
            }
            else if (addProductsDetails.ProductName.Length > 255)
            {
                _logger.LogError("ProductName must be at most 255 characters long \n");

                return StatusCode(400, new ApiResponse<string>(400, "ProductName must be at most 255 characters long."));
            }

            _logger.LogInformation("ProductName validation passed \n");

            _logger.LogInformation("Price validation started \n");

            if (addProductsDetails.Price == 0)
            {
                _logger.LogError("Price can not be null \n");

                return StatusCode(400, new ApiResponse<string>(400, "Price can not be null"));
            }
            else if (addProductsDetails.Price < 0 || addProductsDetails.Price > 999999.99m)
            {
                _logger.LogError("Price must be between 0.01 and 999999.99 \n");

                return StatusCode(400, new ApiResponse<string>(400, "Price must be between 1 and 999999.99."));
            }

            _logger.LogInformation("Price validation passed \n");

            _logger.LogInformation("StockQuantity validation started \n");

            if (addProductsDetails.StockQuantity == 0)
            {
                _logger.LogError("StockQuantity is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "StockQuantity is required."));
            }
            else if (addProductsDetails.StockQuantity < 1 || addProductsDetails.StockQuantity > 10000)
            {
                _logger.LogError("StockQuantity must be between 1 and 10000 \n");

                return StatusCode(400, new ApiResponse<string>(400, "StockQuantity must be between 1 and 10000"));
            }

            _logger.LogInformation("StockQuantity validation passed \n");

            _logger.LogInformation("ImageData validation started \n");

            if (addProductsDetails.ImageData == null || addProductsDetails.ImageData.Length == 0)
            {
                _logger.LogError("Image is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "Image can not be null"));
            }

            if (addProductsDetails.ImageData != null || addProductsDetails.ImageData.Length != 0)
            {
                string contentType = addProductsDetails.ImageData.ContentType;

                if (contentType != null)
                {
                    if (!(contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase)) &&
                        !(contentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase)) &&
                        !(contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogError("Invalid file format please upload a JPG or PNG or JPEG file \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid file format please upload a JPG or PNG or JPEG file"));
                    }
                }
                byte[] imageData;

                using (var memoryStream = new MemoryStream())
                {
                    addProductsDetails.ImageData.CopyTo(memoryStream);
                    imageData = memoryStream.ToArray();
                    using (Image image = Image.Load(imageData))
                    {
                        if (image.Width < 500 || image.Width > 505 || image.Height < 500 || image.Height > 505)
                        {
                            _logger.LogError("Invalid image resolution. Please upload an image with resolution 500 x 500 \n");
                            return StatusCode(400, new ApiResponse<string>(400, "Invalid image resolution. Please upload an image with resolution 500 x 500"));
                        }
                    }
                }
            }

            _logger.LogInformation("ImageData validation passed \n");

            _logger.LogInformation("Product description validation started \n");

            if (string.IsNullOrEmpty(addProductsDetails.ProductDescription))
            {
                _logger.LogError("ProductDescription is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "ProductDescription is required."));
            }
            else if (addProductsDetails.ProductDescription.Length <= 150)
            {
                _logger.LogError("ProductDescription must be at least 150 characters long \n");

                return StatusCode(400, new ApiResponse<string>(400, "ProductDescription must be at least 150 characters long."));
            }
            _logger.LogInformation("Product description validation completed \n");

            _logger.LogInformation("Validation completed in controller action method CreateUpdateProducts \n");
            try
            {
                _logger.LogInformation($"Creating-updating new products starts in controller action method CreateUpdateProducts for product name : {addProductsDetails.ProductName} \n");

                int insertUpdateStatus = await _productsRepository.InsertUpdateProductsAsync(addProductsDetails);

                if (insertUpdateStatus != 0)
                {
                    _logger.LogInformation($"Creating-updating new products completed in controller action method CreateUpdateProducts for product name : {addProductsDetails.ProductName} \n");

                    if (addProductsDetails.ProductId == "0")
                    {
                        _logger.LogInformation($"New product added successfully in controller action method for product name : {addProductsDetails.ProductName} \n");

                        return StatusCode(201, new ApiResponse<string>(201, "Products added successfully."));
                    }
                    else
                    {
                        _logger.LogInformation($"Product updated successfully in controller action method for product name : {addProductsDetails.ProductName} \n");

                        return StatusCode(200, new ApiResponse<string>(200, "Product updated successfully."));
                    }
                }
                else
                {
                    if (addProductsDetails.ProductId == "0")
                    {
                        _logger.LogError($"Insert operation failed unexpectedly in controller action method for product name : {addProductsDetails.ProductName} \n");

                        return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                    }
                    else
                    {
                        _logger.LogError($"Update operation failed unexpectedly in controller action method for product name : {addProductsDetails.ProductName} \n");

                        return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                    }
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("Category does not exist."))
                    {
                        _logger.LogError($"Creating-updating new products failed in controller action method CreateUpdateProducts for product name : {addProductsDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Category does not exist."));
                    }
                    else if (sqlException.Message.Contains("ProductName cannot be NULL or empty."))
                    {
                        _logger.LogError($"Creating-updating new products failed in controller action method CreateUpdateProducts for product name : {addProductsDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "ProductName cannot be NULL or empty."));
                    }
                    else if (sqlException.Message.Contains("Product name already exists."))
                    {
                        _logger.LogWarning($"Creating-updating new products failed given product name already exists in controller action method CreateUpdateProducts for product name : {addProductsDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(409, new ApiResponse<string>(409, "Product name already exists"));
                    }
                    else if (sqlException.Message.Contains("Price must be a positive value."))
                    {
                        _logger.LogError($"Creating-updating new products failed in controller action method CreateUpdateProducts for product name : {addProductsDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Price must be a positive value."));
                    }
                    else if (sqlException.Message.Contains("StockQuantity must be a non-negative value."))
                    {
                        _logger.LogError($"Creating-updating new products failed in controller action method CreateUpdateProducts for product name : {addProductsDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid stock quantity please check and try again."));
                    }
                    else if (sqlException.Message.Contains("Product image required."))
                    {
                        _logger.LogError($"Creating-updating new products failed in controller action method CreateUpdateProducts for product name : {addProductsDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Image required"));
                    }

                    else if (sqlException.Message.Contains("Admin details required."))
                    {
                        _logger.LogError($"Creating-updating new products failed in controller action method CreateUpdateProducts for product name : {addProductsDetails.ProductName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Admin details required."));
                    }
                }
                _logger.LogError($"SQL Error during product create update in controller action method CreateUpdateProducts for product name : {addProductsDetails.ProductName} : {sqlException.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during product create update in controller action method CreateUpdateProducts for product name : {addProductsDetails.ProductName} : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }


        /// <summary>
        /// Deletes a product by its Id.
        /// </summary>
        /// <param name="productId">The Id of the product to delete.</param>
        /// <returns>Returns a response indicating the result of the deletion process.</returns>
        /// <response code="204">Returns a No Content response if the product is successfully deleted.</response>
        /// <response code="400">Returns a Bad Request response if the provided product Id is invalid or missing.</response>
        /// <response code="404">Returns a Not Found response if no product with the provided Id is found.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during the deletion process.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     DELETE /api/products/delete-products/{productId}
        /// 
        /// </example>
        [HttpDelete("delete-products/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProductById(int productId)
        {
            _logger.LogInformation("Validation started in controller action method DeleteProductById \n");

            if (productId == 0)
            {
                _logger.LogError("ProductId is required  \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid product..please check and try again."));
            }

            _logger.LogInformation("Validation completed in controller action method DeleteProductById \n");

            try
            {
                _logger.LogInformation($"Deleting products starts in controller action method DeleteProductById for productId : {productId} \n");

                int deletedProduct = await _productsRepository.DeleteProductByIdAsync(productId);

                if (deletedProduct == 1)
                {
                    _logger.LogInformation($"Product deleted Successfully in controller action method DeleteProductById for productId : {productId} \n");

                    return StatusCode(204, null);
                }
                else
                {
                    _logger.LogWarning($"Problem while deletion product in controller action method DeleteProductById with productId : {productId} \n");
                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("ProductId parameter is required."))
                    {
                        _logger.LogError($"Deleting products falied in controller action method DeleteProductById for productId : {productId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid product data please check and try again."));
                    }
                    else if (sqlException.Message.Contains("Invalid ProductId. Product with such ProductId does not exist."))

                    {
                        _logger.LogWarning($"Product not found in controller action method DeleteProductById for productId : {productId} : {sqlException.Message} \n");
                        return StatusCode(404, new ApiResponse<string>(404, "Product not exists"));
                    }
                }

                _logger.LogError($"SQL error during product deletion in controller action method DeleteProductById for productId : {productId} : {sqlException.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during product deletion in controller action method DeleteProductById for productId : {productId} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }

    }
}

