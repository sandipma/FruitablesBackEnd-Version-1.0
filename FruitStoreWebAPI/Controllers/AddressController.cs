using CommonHelperUtility;
using FruitStoreModels.Address;
using FruitStoreModels.Response;
using FruitStoreRepositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FruitStoreWebAPI.Controllers
{
    /// <summary>
    /// Represents a class that handles address related operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorizeAttribute("user")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressDetailsReposiotry _addressRepository;
        private readonly ILogger<AddressController> _logger;

        /// <summary>
        /// Represents a class that handles address related operations.
        /// </summary>
        public AddressController(ILogger<AddressController> logger, IAddressDetailsReposiotry addressRepository)
        {
            _logger = logger;
            _addressRepository = addressRepository;
        }

        /// <summary>
        /// Retrieves details for all addresses by its ID.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves details for all addresses by its ID available in the system.
        /// </remarks>
        /// <param name="userId">The ID of the address to delete.</param>
        /// <returns>Returns a response containing details for all addresses by its ID.</returns>
        /// <response code="200">Returns a successful response along with address details.</response>
        /// <response code="400">Returns a bad request.</response>
        /// <response code="404">Returns a Not Found response if no addresses are found.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     GET /api/address/get-all-addresses/{userId}
        /// 
        /// </example>
        [HttpGet("get-all-addresses/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<AddressDetails>>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GetAllAddressDetailsByUserId(int userId)
        {
            _logger.LogInformation("Validation started in controller action method GetAllAddressDetailsByUserId \n");

            if (userId == 0)
            {
                _logger.LogError("UserId is required  \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid user..please check and try again."));
            }
            _logger.LogInformation("Validation completed in controller action method GetAllAddressDetailsByUserId \n");
            try
            {
                _logger.LogInformation("Fetching details for address starts in controller action method GetAllAddressDetailsByUserId \n");

                var addressDetails = await _addressRepository.GetAllAddressDetailsByUserIdAsync(userId);

                if (addressDetails != null)
                {
                    _logger.LogInformation("Fetching details for address succeeded in controller action method GetAllAddressDetailsByUserId \n");
                    return StatusCode(200, new ApiResponse<List<AddressDetails>>(200, "Address details..", addressDetails));
                }
                else
                {
                    _logger.LogInformation("Fetching details for address failed no address availabe in controller action method GetAllAddressDetailsByUserId \n");
                    return StatusCode(200, new ApiResponse<List<AddressDetails>>(200, "No address available", null));
                }
            }
            catch (SqlException sqlEx)

            {
                if (sqlEx.Number == 50000)
                {
                    if (sqlEx.Message.Contains("User Id required."))
                    {
                        _logger.LogError($"Fetching details for address failed in controller action method GetAllAddressDetailsByUserId : {sqlEx.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "User Id required."));
                    }
                    if (sqlEx.Message.Contains("Invalid User Id. User with such User Id does not exist."))
                    {
                        _logger.LogError($"Fetching details for address failed in controller action method GetAllAddressDetailsByUserId : {sqlEx.Message} \n");

                        return StatusCode(404, new ApiResponse<string>(404, "Invalid user..Please check & try again"));
                    }
                }
                _logger.LogError($"SQL Error during address fetching in controller action method GetAllAddressDetails : {sqlEx.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during address fetching in controller action method GetAllAddressDetails : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }

        /// <summary>
        /// Creates a new address.
        /// </summary>
        /// <remarks>
        /// This endpoint creates a new address with the provided details.
        /// </remarks>
        /// <param name="addaddressDetails">The details of the address to be created.</param>
        /// <returns>Returns a response indicating the success or failure of the operation.</returns>
        /// <response code="201">Returns a successful response when the address is created successfully.</response>
        /// <response code="400">Returns a Bad Request response if any validation errors occur.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     POST /api/address/create-new-address
        ///     {
        ///         "FirstName": "John",
        ///         "LastName": "Doe",
        ///         "StreetAddress": "123 Main St",
        ///         "PostalCode": 12345,
        ///         "City": "New York",
        ///         "State": "NY",
        ///         "PhoneNumber": "1234567890",
        ///         "UserId": 123
        ///     }
        /// 
        /// </example>
        [HttpPost("create-new-address")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> CreateNewAddress(AddAddressDetails addaddressDetails)
        {
            _logger.LogInformation("Validation started in controller action method CreateNewAddress \n");

            _logger.LogInformation("FirstName validation started \n");

            if (string.IsNullOrEmpty(addaddressDetails.FirstName))
            {
                _logger.LogError("FirstName is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "FirstName is required."));
            }
            _logger.LogInformation("FirstName validation passed \n");

            _logger.LogInformation("LastName validation started \n");

            if (string.IsNullOrEmpty(addaddressDetails.LastName))
            {
                _logger.LogError("LastName is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "LastName is required."));
            }
            _logger.LogInformation("LastName validation passed \n");

            _logger.LogInformation("StreetAddress validation started \n");

            if (string.IsNullOrEmpty(addaddressDetails.StreetAddress))
            {
                _logger.LogError("StreetAddress is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "StreetAddress is required."));
            }
            _logger.LogInformation("StreetAddress validation passed \n");

            _logger.LogInformation("Email validation started \n");

            if (string.IsNullOrEmpty(addaddressDetails.Email))
            {
                _logger.LogError("Email is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email is required."));
            }

            else if (!UserValidator.IsValidEmail(addaddressDetails.Email))
            {
                _logger.LogError("Invalid email format \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid email format..kindly try with proper email"));
            }
            _logger.LogInformation("Email validation passed \n");

            _logger.LogInformation("PostalCode validation started \n");

            if (addaddressDetails.PostalCode == 0)
            {
                _logger.LogError("PostalCode can not be null \n");

                return StatusCode(400, new ApiResponse<string>(400, "PostalCode can not be null"));
            }
            else if (addaddressDetails.PostalCode.ToString().Length < 6)
            {
                _logger.LogError("PostalCode must be atleast 6 digits \n");

                return StatusCode(400, new ApiResponse<string>(400, "PostalCode must be atleast 6 digits"));
            }
            _logger.LogInformation("PostalCode validation passed \n");

            _logger.LogInformation("City validation started \n");

            if (string.IsNullOrEmpty(addaddressDetails.City))
            {
                _logger.LogError("City is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "City is required."));
            }
            _logger.LogInformation("City validation passed \n");

            _logger.LogInformation("State validation started \n");

            if (string.IsNullOrEmpty(addaddressDetails.State))
            {
                _logger.LogError("State is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "State is required."));
            }
            _logger.LogInformation("State validation passed \n");

            _logger.LogInformation("Phone number validation started \n");

            if (addaddressDetails.PhoneNumber == 0)
            {
                _logger.LogError("PhoneNumber is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "PhoneNumber is required."));
            }
            else if (addaddressDetails.PhoneNumber.ToString().Length < 10)
            {
                _logger.LogError("PhoneNumber is atleast 10 characters long. \n");

                return StatusCode(400, new ApiResponse<string>(400, "PhoneNumber is atleast 10 characters long."));
            }
            _logger.LogInformation("Phone number validation passed \n");

            _logger.LogInformation("UserId validation started \n");

            if (addaddressDetails.UserId == 0)
            {
                _logger.LogError("User details is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "User details is required."));
            }
            _logger.LogInformation("UserId validation passed \n");

            _logger.LogInformation("Country validation started \n");

            if (string.IsNullOrEmpty(addaddressDetails.Country))
            {
                _logger.LogError("Country is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "Country is required."));
            }
            _logger.LogInformation("Country validation passed \n");

            _logger.LogInformation("Validation completed in controller action method CreateNewAddress \n");
            try
            {
                _logger.LogInformation($"Creating new address starts in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} \n");

                int insertStatus = await _addressRepository.InsertAddressDetailsAsync(addaddressDetails);

                if (insertStatus != 0)
                {
                    _logger.LogInformation($"New address added successfully in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} \n");

                    return StatusCode(201, new ApiResponse<string>(201, "Address added successfully."));

                }
                else
                {
                    _logger.LogInformation($"New address failed in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} \n");

                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("User name cannot be null."))
                    {
                        _logger.LogError($"Creating new address failed in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "User name cannot be null."));
                    }
                    else if (sqlException.Message.Contains("User details required."))
                    {
                        _logger.LogError($"Creating new address failed in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "User details required."));
                    }
                    else if (sqlException.Message.Contains("Last name cannot be null."))
                    {
                        _logger.LogError($"Creating new address failed in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Last name cannot be null."));
                    }
                    else if (sqlException.Message.Contains("StreetAddress cannot be null."))
                    {
                        _logger.LogError($"Creating new address failed in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "StreetAddress cannot be null."));
                    }
                    else if (sqlException.Message.Contains("Postal code must be at least 6 digits."))
                    {
                        _logger.LogError($"Creating new address failed in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Postal code must be at least 6 digits."));
                    }
                    else if (sqlException.Message.Contains("City cannot be null."))
                    {
                        _logger.LogError($"Creating new address failed in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "City cannot be null."));
                    }
                    else if (sqlException.Message.Contains("State cannot be null."))
                    {
                        _logger.LogError($"Creating new address failed in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "State cannot be null."));
                    }
                    else if (sqlException.Message.Contains("Phone number must be at least 10 digits."))
                    {
                        _logger.LogError($"Creating new address failed in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Phone number must be at least 10 digits."));
                    }
                    else if (sqlException.Message.Contains("Email cannot be null."))
                    {
                        _logger.LogError($"Creating new address failed in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Email cannot be null."));
                    }
                    else if (sqlException.Message.Contains("Country cannot be null."))
                    {
                        _logger.LogError($"Creating new address failed in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Country cannot be null."));
                    }
                }
                _logger.LogError($"SQL Error during address create in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} : {sqlException.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during address create in controller action method CreateNewAddress for  name : {addaddressDetails.FirstName} : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }

        /// <summary>
        /// Deletes an address by its ID.
        /// </summary>
        /// <remarks>
        /// This endpoint deletes an address with the specified ID.
        /// </remarks>
        /// <param name="addressId">The ID of the address to delete.</param>
        /// <returns>Returns a response indicating the success or failure of the operation.</returns>
        /// <response code="204">Returns a successful response when the address is deleted successfully.</response>
        /// <response code="400">Returns a Bad Request response if the address ID is invalid.</response>
        /// <response code="404">Returns a Not Found response if the address with the specified ID is not found.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     DELETE /api/address/delete-address/{addressId}
        /// 
        /// </example>
        [HttpDelete("delete-address/{addressId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> DeleteAddressById(int addressId)
        {
            _logger.LogInformation("Validation started in controller action method DeleteAddressById \n");

            if (addressId == 0)
            {
                _logger.LogError("AddressId is required  \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid address..please check and try again."));
            }

            _logger.LogInformation("Validation completed in controller action method DeleteAddressById \n");

            try
            {
                _logger.LogInformation($"Deleting address starts in controller action method DeleteAddressById for addressId : {addressId} \n");

                int deletedAddress = await _addressRepository.DeleteAddressByIdAsync(addressId);

                if (deletedAddress == 1)
                {
                    _logger.LogInformation($"Address deleted Successfully in controller action method DeleteAddressById for addressId : {addressId} \n");
                    return StatusCode(204, null);

                }
                else
                {
                    _logger.LogError($"Problem while deletion of address in controller action method DeleteAddressById for addressId : {addressId} \n");
                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("Address Id parameter is required."))
                    {
                        _logger.LogError($"Deleting Address failed in controller action method DeleteAddressById for addressId : {addressId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid address data please check and try again."));
                    }
                    else if (sqlException.Message.Contains("Invalid Address Id. Address with such address Id does not exist."))

                    {
                        _logger.LogWarning($"Address not found in controller action method DeleteAddressById for addressId : {addressId} : {sqlException.Message} \n");
                        return StatusCode(404, new ApiResponse<string>(404, "Address not exists"));
                    }
                }

                _logger.LogError($"SQL error during address deletion in controller action method DeleteAddressById for addressId : {addressId} : {sqlException.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during address deletion in controller action method DeleteAddressById for addressId : {addressId} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }


        /// <summary>
        /// Updates the selection status of an address by its ID.
        /// </summary>
        /// <remarks>
        /// This endpoint updates the selection status of an address with the specified ID.
        /// </remarks>
        /// <param name="addressId">The ID of the address to update selection status for.</param>
        /// <param name="isSelected">The new selection status to update.</param>
        /// <returns>Returns a response indicating the result of the update operation.</returns>
        /// <response code="200">Returns a successful response indicating that the address selection status has been updated successfully.</response>
        /// <response code="400">Returns a Bad Request response if there are validation errors or if the provided data is invalid.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     PATCH /api/address/update-address-selection/{addressId}?isSelected=selected
        /// 
        /// </example>
        [HttpPatch("update-address-selection/{addressId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> UpdateAddressSelectionById(int addressId, [FromQuery] string isSelected)
        {
            _logger.LogInformation($"Validation started in controller action method UpdateAddressSelectionById for addressId : {addressId}  \n");

            _logger.LogInformation("AddressId validation started \n");

            if (addressId == 0)
            {
                _logger.LogError("Address is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "Address is required."));
            }
            _logger.LogInformation("AddressId validation passed \n");

            _logger.LogInformation("isSelected validation started \n");

            if (string.IsNullOrEmpty(isSelected))
            {
                _logger.LogError("No address selected \n");

                return StatusCode(400, new ApiResponse<string>(400, "No address selected."));
            }
            _logger.LogInformation("isSelected validation passed \n");

            _logger.LogInformation($"Validation completed in controller action method UpdateAddressSelectionById for addressId : {addressId} \n");

            try
            {
                _logger.LogInformation($"Updating address selection starts in controller action method UpdateAddressSelectionById for addressId : {addressId} \n");

                int insertStatus = await _addressRepository.UpdateAddressSelectionAsync(addressId, isSelected);

                if (insertStatus != 0)
                {
                    _logger.LogInformation($"Address selection updated successfully in controller action method UpdateAddressSelectionById for addressId : {addressId} \n");

                    return StatusCode(200, new ApiResponse<string>(200, "Address changed successfully."));
                }
                else
                {
                    _logger.LogInformation($"Address selection failed address in controller action method UpdateAddressSelectionById for addressId : {addressId} \n");

                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("Address details cannot be NULL or empty."))
                    {
                        _logger.LogError($"Updating address selection failed in controller action method UpdateAddressSelectionById for addressId : {addressId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Address details required"));
                    }
                    else if (sqlException.Message.Contains("Selection details cannot be NULL or empty."))
                    {
                        _logger.LogError($"Updating address selection failed in controller action method UpdateAddressSelectionById for addressId : {addressId} : {sqlException.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Kindly select the address"));
                    }
                    else if (sqlException.Message.Contains("Invalid Address Id. Address with such address Id does not exist."))
                    {
                        _logger.LogError($"Updating address selection failed in controller action method UpdateAddressSelectionById for addressId : {addressId} : {sqlException.Message} \n");

                        return StatusCode(404, new ApiResponse<string>(404, "Address not exists"));
                    }
                }
                _logger.LogError($"SQL Error during updating address selection failed in controller action method UpdateAddressSelectionById for addressId : {addressId} : {sqlException.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during updating address selection failed in controller action method UpdateAddressSelectionById for addressId : {addressId} : {ex.Message} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }

        }
    }
}
