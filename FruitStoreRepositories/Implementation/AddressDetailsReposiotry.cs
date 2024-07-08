using Dapper;
using FruitStoreModels.Address;
using FruitStoreRepositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Data;

namespace FruitStoreRepositories.Implementation
{
    public class AddressDetailsReposiotry : IAddressDetailsReposiotry
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AddressDetailsReposiotry> _logger;
        public AddressDetailsReposiotry(IConfiguration configuration, ILogger<AddressDetailsReposiotry> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        //... Method for delete address by address Id...//
        public async Task<int> DeleteAddressByIdAsync(int addressId)
        {
            try
            {
                _logger.LogInformation($"Deleting records for Address starts in method DeleteAddressByIdAsync Id : {addressId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@AddressId ", addressId);
                    parameters.Add("@DeleteResult", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    _logger.LogInformation("Executing stored procedure stp_DeleteAddressById \n");

                    await dbConnection.ExecuteAsync(
                        "stp_DeleteAddressById",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    int deleteStatus = parameters.Get<int>("@DeleteResult");

                    if (deleteStatus == 1)
                    {
                        _logger.LogInformation($"Deleting details for address from DB succeeded in method DeleteAddressByIdAsync for addressId : {addressId} \n");

                    }
                    else
                    {
                        _logger.LogError($"Address Deletion failed from DB in method DeleteAddressByIdAsync for addressId : {addressId} \n");
                    }
                    return deleteStatus;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while Deleting data for address in method DeleteAddressByIdAsync for addressId : {addressId} : {ex.Message} \n");
                throw;
            }
        }

        //... Method for get all addresses details...//
        public async Task<List<AddressDetails>> GetAllAddressDetailsByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching details for address starts in method GetAllAddressDetailsByUserIdAsync \n");

                List<AddressDetails> addressLists = null;

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    _logger.LogInformation("Executing stored procedure stp_GetAllAdressListByUserId \n");

                    var parameters = new DynamicParameters();

                    parameters.Add("@UserId", userId);

                    var addressDetails = await dbConnection.QueryAsync<AddressDetails>(
                        "stp_GetAllAdressListByUserId",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (addressDetails.Any())
                    {
                        _logger.LogInformation("Fetching details for address from DB succeeded in method GetAllAddressDetailsByUserIdAsync \n");
                        addressLists = addressDetails.ToList();
                    }
                    else
                    {
                        _logger.LogError("No address details found from DB in method GetAllAddressDetailsByUserIdAsync \n");
                    }

                    return addressLists;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching data for address in method GetAllAddressDetailsByUserIdAsync : {ex.Message} \n");
                throw;
            }
        }

        //... Method for insert new address details...//
        public async Task<int> InsertAddressDetailsAsync(AddAddressDetails addAddressDetails)
        {
            try
            {
                _logger.LogInformation($"Inserting details for address starts in method InsertAddressDetailsAsync for user name : {addAddressDetails.FirstName} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@FirstName", addAddressDetails.FirstName);
                    parameters.Add("@LastName", addAddressDetails.LastName);
                    parameters.Add("@StreetAddress", addAddressDetails.StreetAddress);
                    parameters.Add("@PostalCode", addAddressDetails.PostalCode);
                    parameters.Add("@City", addAddressDetails.City);
                    parameters.Add("@State", addAddressDetails.State);
                    parameters.Add("@Country", addAddressDetails.Country);
                    parameters.Add("@PhoneNumber", addAddressDetails.PhoneNumber);
                    parameters.Add("@Email", addAddressDetails.Email);
                    parameters.Add("@UserId", addAddressDetails.UserId);

                    _logger.LogInformation("Executing stored procedure stp_SetAddressDetails...");

                    var addressId = await dbConnection.ExecuteScalarAsync<int>(
                        "stp_SetAddressDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (addressId != 0)
                    {
                        _logger.LogInformation($"Address added successfully into DB in method InsertAddressDetailsAsync for name : {addAddressDetails.FirstName} \n");
                    }
                    else
                    {
                        _logger.LogError($"Problem while inserting data to the DB in method InsertAddressDetailsAsync for name : {addAddressDetails.FirstName} \n");
                    }
                    return addressId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while inserting data address details in method InsertAddressDetailsAsync : {addAddressDetails.FirstName} : {ex.Message} \n");
                throw;
            }
        }

        //...Method for update isAddressSelected patch ...//
        public async Task<int> UpdateAddressSelectionAsync(int addresssId, string isAddressSelected)
        {
            try
            {
                _logger.LogInformation($"Updating details for isAddressSelected starts in method UpdateAddressSelectionAsync for addressId : {addresssId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@AddressId", addresssId);
                    parameters.Add("@IsSelectedFlag", isAddressSelected);

                    _logger.LogInformation("Executing stored procedure stp_UpdateAddressSelectionById...");

                    var addressId = await dbConnection.ExecuteScalarAsync<int>(
                        "stp_UpdateAddressSelectionById",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (addressId != 0)
                    {
                        _logger.LogInformation($"Address updated successfully into DB in method UpdateAddressSelectionAsync for addressId : {addresssId} \n");
                    }
                    else
                    {
                        _logger.LogError($"Problem while updating data to the DB in not found address to update in method UpdateAddressSelectionAsync for addressId : {addresssId} \n");
                    }
                    return addressId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating data in method UpdateAddressSelectionAsync for addressId : {addresssId} : {ex.Message} \n");
                throw;
            }
        }

    }
}
