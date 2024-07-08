using CommonHelperUtility;
using FruitStoreModels.Category;
using FruitStoreModels.Response;
using FruitStoreRepositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FruitStoreWebAPI.Controllers
{
    /// <summary>
    /// Represents a class that handles category related operations.
    /// </summary>
   
    [Route("api/[controller]")]
    [ApiController]

    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryController> _logger;

        /// <summary>
        /// Represents a class that handles category related operations.
        /// </summary>
        public CategoryController(ILogger<CategoryController> logger, ICategoryRepository categoryRepository)
        {

            _logger = logger;
            _categoryRepository = categoryRepository;

        }

        /// <summary>
        /// Retrieves details for all categories in the system.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves details for all categories available in the system.
        /// </remarks>
        /// <returns>Returns a response containing the list of categories.</returns>
        /// <response code="200">Returns a successful response with the list of categories.</response>
        /// <response code="500">Returns an Internal server error response if an unexpected error occurs during retrieval.</response>
        /// <summary>
        /// Sample request:
        ///     GET /api/category/get-all-categories
        /// </summary>   
        [HttpGet("get-all-categories")]
        [CustomAuthorizeAttribute("admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<CategoryDetails>>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetAllCateGories()
        {
            try
            {
                _logger.LogInformation("Fetching details for categories starts in controller action method GetAllCateGories \n");

                var categoryDetails = await _categoryRepository.GetAllCategoriesAsync();

                if (categoryDetails != null)
                {
                    _logger.LogInformation("Fetching details for categories succeeded in controller action method GetAllCateGories \n");
                    return StatusCode(200, new ApiResponse<List<CategoryDetails>>(200, "Category details..", categoryDetails));
                }
                else
                {
                    _logger.LogError("No categories found in controller action method GetAllCateGories \n");
                    return StatusCode(200, new ApiResponse<List<CategoryDetails>>(200, "No category found..", null));
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL error during category fetching in controller action method GetAllCateGories : {sqlEx.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during category fetching in controller action method GetAllCateGories : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
        }


        /// <summary>
        /// Retrieves details for categories along with the count of products in each category.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves details for categories along with the count of products in each category.
        /// </remarks>
        /// <returns>Returns a response containing the list of categories with product counts.</returns>
        /// <response code="200">Returns a successful response with the list of categories and their product counts.</response> 
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during retrieval.</response>
        /// <summary>
        /// Sample request:
        ///     GET /api/category/get-category-with-products-counts
        /// </summary>    
        
        [HttpGet("get-category-with-products-counts")]
        [CustomAuthorizeAttribute("user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<CategoryWithProductsCounts>>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetCategoryWithProductsCounts()
        {
            try
            {
                _logger.LogInformation("Fetching details for categories with products count starts in controller action method GetCategoryWithProductsCounts \n");

                var categoryWithProductCount = await _categoryRepository.GetCategoryWithProductsCountsAsync();

                if (categoryWithProductCount != null)
                {
                    _logger.LogInformation("Fetching details for categories with products count succeeded in controller action method GetCategoryWithProductsCounts \n");

                    return StatusCode(200, new ApiResponse<List<CategoryWithProductsCounts>>(200, "Category with product count details..", categoryWithProductCount));
                }
                else
                {
                    _logger.LogError("No categories with products count found in controller action method GetCategoryWithProductsCounts \n");

                    return StatusCode(200, new ApiResponse<List<CategoryWithProductsCounts>>(200, "Category with product count details..", null));
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during category fetching with product count in controller action method GetCategoryWithProductsCounts : {sqlEx.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during category fetching with product count in controller action method GetCategoryWithProductsCounts : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
        }


        /// <summary>
        /// Creates a new category in the system.
        /// </summary>
        /// <remarks>
        /// This endpoint allows adding a new category to the system.
        /// </remarks>
        /// <param name="addcategoryDetails">The details of the category to be created.</param>
        /// <returns>Returns a response indicating the success or failure of the category creation.</returns>
        /// <response code="201">Returns a Created response if the category is created successfully.</response>
        /// <response code="400">Returns a Bad Request response if the provided category details are invalid or missing.</response>
        /// <response code="409">Returns a conflict.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during creation.</response>
        /// <sample-request>
        /// POST api/category/create-category
        /// Content-Type: application/json
        /// 
        /// {
        ///   "CategoryName": "New Category",
        ///   "CategoryDescription": "Description of the new category"
        /// }
        /// </sample-request>
        [HttpPost("create-category")]
        [CustomAuthorizeAttribute("admin")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> CreateCategory(AddCategoryDetails addcategoryDetails)
        {
            _logger.LogInformation("Validation started in controller action method CreateCategory \n");

            _logger.LogInformation("CategoryName validation started \n");

            if (string.IsNullOrEmpty(addcategoryDetails.CategoryName))
            {
                _logger.LogError("CategoryName is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "CategoryName is required."));
            }

            else if (addcategoryDetails.CategoryName.Length > 100)
            {
                _logger.LogError("CategoryName must be at most 100 characters long \n");

                return StatusCode(400, new ApiResponse<string>(400, "CategoryName must be at most 100 characters long."));
            }

            _logger.LogInformation("CategoryName validation passed \n");

            _logger.LogInformation("AdminId validation started \n");

            if (addcategoryDetails.AdminId == 0)
            {
                _logger.LogError("AdminId is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "AdminId is required."));
            }

            _logger.LogInformation("AdminId validation passed \n");

            _logger.LogInformation("Validation completed in controller action method CreateCategory \n");

            try
            {
                _logger.LogInformation($"Creating new category starts in controller action method CreateCategory for category : {addcategoryDetails.CategoryName} \n");

                var insertCategoryStatus = await _categoryRepository.InsertNewCategoryAsync(addcategoryDetails);

                if (insertCategoryStatus != 0)
                {
                    _logger.LogInformation($"Creating new category completed in controller action method CreateCategory for category : {addcategoryDetails.CategoryName} \n");

                    return StatusCode(201, new ApiResponse<string>(201, "Category added successfully."));
                }
                else
                {
                    _logger.LogError($"Creating new category failed in controller action method CreateCategory for category : {addcategoryDetails.CategoryName} \n");

                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("category cannot be NULL or empty."))
                    {
                        _logger.LogError($"Creating new category failed in controller action method CreateCategory for category : {addcategoryDetails.CategoryName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Category cannot be NULL"));
                    }

                    else if (sqlException.Message.Contains("Category already exists."))
                    {
                        _logger.LogWarning($"Category already exists in controller action method CreateCategory for category : {addcategoryDetails.CategoryName} : {sqlException.Message} \n");

                        return StatusCode(409, new ApiResponse<string>(409, "Category already exists"));
                    }

                    else if (sqlException.Message.Contains("Admin details required."))
                    {
                        _logger.LogError($"Creating new category failed in controller action method CreateCategory for category : {addcategoryDetails.CategoryName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Admin details required."));
                    }
                }
                _logger.LogError($"SQL error during category insertion in controller action method CreateCategory for category : {addcategoryDetails.CategoryName} : {sqlException.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during category insertion in controller action method CreateCategory for category : {addcategoryDetails.CategoryName} : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
        }


        /// <summary>
        /// Deletes a category by its Id.
        /// </summary>
        /// <param name="categoryId">The Id of the category to delete.</param>
        /// <returns>Returns a response indicating the result of the deletion process.</returns>
        /// <response code="204">Returns a No Content response if the category is successfully deleted.</response>
        /// <response code="400">Returns a Bad Request response if the provided category Id is invalid or missing.</response>
        /// <response code="404">Returns a Not Found response if no category with the provided Id is found.</response>
        /// <response code="409">Returns a Conflict response if the category cannot be deleted because it contains products.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during the deletion process.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     DELETE /api/category/delete-category/{categoryId}
        /// 
        /// </example>
        [HttpDelete("delete-category/{categoryId}")]
        [CustomAuthorizeAttribute("admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> DeleteCategoryById(int categoryId)
        {
            _logger.LogInformation("Validation started in controller action method DeleteCategoryById \n");

            if (categoryId == 0)
            {
                _logger.LogError("CategoryId is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid category..please check and try again."));
            }

            _logger.LogInformation("Validation completed in controller action method DeleteCategoryById \n");

            try
            {
                _logger.LogInformation($"Deleting category starts in controller action method DeleteCategoryById for categoryId : {categoryId} \n");

                int deletedCategoryStatus = await _categoryRepository.DeleteCategoryByIdAsync(categoryId);

                if (deletedCategoryStatus == 1)
                {
                    _logger.LogInformation($"Category deleted Successfully in controller action method DeleteCategoryById for categoryId : {categoryId} \n");
                    return StatusCode(204, null);
                }
                else
                {
                    _logger.LogError($"Category deletion failed in controller action method DeleteCategoryById for categoryId : {categoryId} \n");
                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                }

            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("Category Id parameter is required."))
                    {
                        _logger.LogError($"Deleting category failed in controller action method DeleteCategoryById for categoryId : {categoryId} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid category data... Please check and try again."));
                    }
                    else if (sqlException.Message.Contains("Invalid categoryId. Category with such categoryId does not exist."))
                    {
                        _logger.LogError($"Category not found in controller action method DeleteCategoryById for categoryId : {categoryId} : {sqlException.Message} \n");
                        return StatusCode(404, new ApiResponse<string>(404, "Category not exists"));
                    }
                    else if (sqlException.Message.Contains("You can not delete this Category kindly delete products under this category."))
                    {
                        _logger.LogWarning($"You can not delete this Category kindly delete products under this category in controller action method DeleteCategoryById for categoryId : {categoryId} : {sqlException.Message} \n");
                        return StatusCode(409, new ApiResponse<string>(409, "You can not delete this Category kindly delete products under this category."));
                    }
                }

                _logger.LogError($"SQL Error during category deletion in controller action method DeleteCategoryById for categoryId : {categoryId} : {sqlException.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during during category deletion in controller action method DeleteCategoryById  for categoryId : {categoryId} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }
    }

}