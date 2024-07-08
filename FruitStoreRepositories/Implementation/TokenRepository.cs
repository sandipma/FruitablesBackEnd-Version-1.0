using Dapper;
using FruitStoreModels.ApplicationUser;
using FruitStoreModels.Auth;
using FruitStoreRepositories.InterFaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;

namespace FruitStoreRepositories.Implementation
{
    public class TokenRepository : ITokenRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenRepository> _logger;
        public TokenRepository(IConfiguration configuration, ILogger<TokenRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;

        }

        //...Method for delete token details by user email...//
        public async Task<int> DeleteTokenDetailsByMailAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Deleting records for token starts in method DeleteTokenDetailsByMailAsync for email : {email} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@Email", email);
                    parameters.Add("@DeleteResult", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    _logger.LogInformation("Executing stored procedure stp_DeleteTokenDetailsByMail \n");

                    await dbConnection.ExecuteAsync(
                        "stp_DeleteTokenDetailsByMail",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    int deleteStatus = parameters.Get<int>("@DeleteResult");

                    if (deleteStatus == 0)
                    {
                        _logger.LogError($"Records for token deletion failed in method DeleteTokenDetailsByMailAsync email : {email} \n");
                    }
                    else
                    {
                        _logger.LogInformation($"Records for token deletion completed successfully in method DeleteTokenDetailsByMailAsync email : {email} \n");
                    }

                    return deleteStatus;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while Deleting data for tokens in method DeleteTokenDetailsByMailAsync email : {email} : {ex.Message} \n");
                throw;
            }

        }

        //...Method for delete both token on logout by user email...//
        public async Task<int> DeleteTokenDetailsOnLogoutByMailAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Deleting records for both token starts method : DeleteTokenDetailsOnLogoutByMailAsync email : {email} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@Email", email);
                    parameters.Add("@DeleteResult", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    _logger.LogInformation("Executing stored procedure stp_DeleteTokenDetailsOnLogout \n");

                    await dbConnection.ExecuteAsync(
                        "stp_DeleteTokenDetailsOnLogout",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    int deleteStatus = parameters.Get<int>("@DeleteResult");

                    if (deleteStatus == 0)
                    {
                        _logger.LogError($"Records for token deletion failed method deleteTokenDetailsByMailAsync email : {email} \n");
                    }
                    else
                    {
                        _logger.LogInformation($"Records for token deletion completed successfully method  deleteTokenDetailsByMailAsyn email : {email} \n");
                    }

                    return deleteStatus;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while deleting data for tokens from the database in method  DeleteTokenDetailsOnLogoutByMailAsync email : {email} : {ex.Message} \n");
                throw;
            }

        }

        //...Method for generate access token using user details...//
        public async Task<AccessTokenDetails> GenerateJwtTokenDetailsAsync(UserDetails userDetails, string token)
        {
            try
            {
                _logger.LogInformation($"Attempting to get access token details starts in method GenerateJwtTokenDetailsAsync starts user : {userDetails.UserName} \n");

                var accessTokenDetails = await GetAccessTokenByEmailDetailsAsync(userDetails.Email);

                if (accessTokenDetails == null)
                {
                    _logger.LogInformation($"Access token retrieved successfully for user : {userDetails.UserName} \n");

                    if (token != null)
                    {
                        _logger.LogInformation($"Attempting to insert access token details for user : {userDetails.UserName} \n");

                        var accessTokenEntity = new InsertAccessTokenDetails()
                        {
                            Email = userDetails.Email,
                            AccessToken = token,
                            AccessTokenTimePeriod = DateTime.Now.AddMinutes(120)
                        };

                        var email = await InsertAccessTokenDetailsAsync(accessTokenEntity);

                        if (email != null)
                        {
                            _logger.LogInformation($"Access token details inserted successfully for user : {userDetails.UserName} \n");

                            _logger.LogInformation($"Attempting to get access token details by email for user : {userDetails.UserName} \n");

                            var tokenDetails = await GetAccessTokenByEmailDetailsAsync(email);

                            if (tokenDetails != null)
                            {
                                _logger.LogInformation($"Attempting to get access token details completed successfully in method GenerateJwtTokenDetailsAsync for user : {userDetails.UserName} \n");
                                return tokenDetails;
                            }
                        }
                    }
                }
                else if (accessTokenDetails != null && accessTokenDetails.AccessTokenTimePeriod > DateTime.Now)
                {
                    _logger.LogInformation($"Attempting to get access token details completed successfully in method GenerateJwtTokenDetailsAsync for user : {userDetails.UserName} \n");
                    return accessTokenDetails;
                }
                _logger.LogError($"Access token details not found or expired for user : {userDetails.UserName} \n");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during JWT token generation in method GenerateJwtTokenDetailsAsync for user : {userDetails.UserName} : {ex.Message} \n");
                throw;
            }
        }

        //...Method for generate refresh token using user details...//
        public async Task<RefreshTokenDetails> GenerateRefreshTokenDetailsAsync(UserDetails userDetails, string token)
        {
            try
            {
                _logger.LogInformation($"Attempting to get refresh token details for user : {userDetails.UserName} \n");

                var refreshTokenDetails = await GetRefreshTokenByEmailDetailsAsync(userDetails.Email);

                if (refreshTokenDetails == null)
                {
                    _logger.LogInformation($"Refresh token retrieved successfully for user : {userDetails.UserName} \n");

                    if (token != null)
                    {
                        _logger.LogInformation($"Generated refresh token successfully for user : {userDetails.UserName} \n");

                        var refreshTokenEntity = new InsertRefreshTokenDetails()
                        {
                            Email = userDetails.Email,
                            RefreshToken = token,
                            RefreshTokenTimePeriod = DateTime.Now.AddMinutes(125)
                        };

                        _logger.LogInformation($"Attempting to insert refresh token details for user : {userDetails.UserName} \n");

                        var email = await InsertRefreshTokenDetailsAsync(refreshTokenEntity);

                        if (email != null)
                        {
                            _logger.LogInformation($"Refresh token details inserted successfully for user : {userDetails.UserName} \n");

                            _logger.LogInformation($"Attempting to get refresh token details by email for user : {userDetails.UserName} \n");

                            var tokenDetails = await GetRefreshTokenByEmailDetailsAsync(email);

                            if (tokenDetails != null)
                            {
                                _logger.LogInformation($"Refresh token retrieved successfully for user : {userDetails.UserName} \n");
                                return tokenDetails;
                            }
                        }
                    }
                }
                else if (refreshTokenDetails != null && refreshTokenDetails.RefreshTokenTimePeriod > DateTime.Now)
                {
                    _logger.LogInformation($"Refresh token retrieved successfully for user : {userDetails.UserName} \n");
                    return refreshTokenDetails;
                }

                _logger.LogError($"Refresh token details not found or expired for user : {userDetails.UserName} \n");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during JWT refresh token generation for user : {userDetails.UserName} : {ex.Message} \n");
                throw;
            }

        }

        //...Method for get details of access token from provided email...//
        public async Task<AccessTokenDetails> GetAccessTokenByEmailDetailsAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Access Token retrieval attempt for method GetAccessTokenByEmailDetailsAsync starts  email : {email} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new
                    {
                        Email = email
                    };

                    _logger.LogInformation("Executing stored procedure stp_GetAccessTokenDetailsByEmail \n");

                    var tokenDetails = await dbConnection.QueryFirstOrDefaultAsync<AccessTokenDetails>(
                        "stp_GetAccessTokenDetailsByEmail",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (tokenDetails == null)
                    {
                        _logger.LogError($"No Access Token present in DB in method GetAccessTokenByEmailDetailsAsync email : {email} \n");
                    }
                    else
                    {
                        _logger.LogInformation($"Access Token retrieved from the DB successfully in method GetAccessTokenByEmailDetailsAsync  email : {email} \n");
                    }

                    return tokenDetails;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving accessToken in method GetAccessTokenByEmailDetailsAsync email : {email} : {ex.Message} \n");
                throw;
            }
        }

        //...Method for get details of refresh token from provided email...//
        public async Task<RefreshTokenDetails> GetRefreshTokenByEmailDetailsAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Refresh token retrieval attempt for method GetRefreshTokenByEmailDetailsAsync starts email : {email} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new
                    {
                        Email = email
                    };

                    _logger.LogInformation("Executing stored procedure stp_GetRefreshTokenDetailsByEmail \n");

                    var refreshToken = await dbConnection.QueryFirstOrDefaultAsync<RefreshTokenDetails>(
                        "stp_GetRefreshTokenDetailsByEmail",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (refreshToken == null)
                    {
                        _logger.LogError($"No Refresh Token present in DB... in method GetAccessTokenByEmailDetailsAsync email : {email} \n");
                    }
                    else
                    {
                        _logger.LogInformation($"Refresh token retrieved from the DB successfully in method GetRefreshTokenByEmailDetailsAsync email : {email} \n");
                    }

                    return refreshToken;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving refresh token in method GetRefreshTokenByEmailDetailsAsync  email : {email} : {ex.Message} \n");
                throw;
            }
        }

        //...Method for insert access token details for user...//
        public async Task<string> InsertAccessTokenDetailsAsync(InsertAccessTokenDetails insertAccessTokenDetails)
        {
            try
            {
                _logger.LogInformation($"Inserting access token details for method InsertAccessTokenDetailsAsync starts for token Email : {insertAccessTokenDetails.Email} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("Email", insertAccessTokenDetails.Email);
                    parameters.Add("AccessToken", insertAccessTokenDetails.AccessToken);
                    parameters.Add("AccessTokenTimePeriod", insertAccessTokenDetails.AccessTokenTimePeriod);
                    parameters.Add("InsertedEmail", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                    _logger.LogInformation("Executing stored procedure stp_SetAccessTokenDetails \n");

                    await dbConnection.ExecuteAsync(
                        "stp_SetAccessTokenDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                    _logger.LogInformation("Stored procedure completed successfully \n");

                    var email = parameters.Get<string>("InsertedEmail");

                    if (email != null)
                    {
                        _logger.LogInformation($"AccessToken details inserted into the DB successfully in method InsertAccessTokenDetailsAsync for email : {insertAccessTokenDetails.Email} \n");
                    }
                    else
                    {
                        _logger.LogError($"AccessToken details falied to insert into the DB in method InsertAccessTokenDetailsAsync for email : {insertAccessTokenDetails.Email} \n");
                    }

                    return email;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting AccessToken details for method InsertAccessTokenDetailsAsync email : {insertAccessTokenDetails.Email} : {ex.Message} \n");

                throw;
            }
        }

        //...Method for insert refresh token details for user...//
        public async Task<string> InsertRefreshTokenDetailsAsync(InsertRefreshTokenDetails insertRefreshTokenDetails)
        {
            try
            {
                _logger.LogInformation($"Inserting refresh token details in method InsertRefreshTokenDetailsAsync starts for email : {insertRefreshTokenDetails.Email} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("Email", insertRefreshTokenDetails.Email);
                    parameters.Add("RefreshToken", insertRefreshTokenDetails.RefreshToken);
                    parameters.Add("RefreshTokenTimePeriod", insertRefreshTokenDetails.RefreshTokenTimePeriod);
                    parameters.Add("InsertedEmail", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                    _logger.LogInformation("Executing stored procedure stp_SetRefreshTokenDetails \n");

                    await dbConnection.ExecuteAsync(
                        "stp_SetRefreshTokenDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    var email = parameters.Get<string>("InsertedEmail");

                    if (email != null)
                    {
                        _logger.LogInformation($"RefreshToken details inserted into the DB successfully in method InsertRefreshTokenDetailsAsync email : {insertRefreshTokenDetails.Email} \n");
                    }
                    else
                    {
                        _logger.LogError($"RefreshToken details failed to insert into the DB in method InsertAccessTokenDetailsAsync for email : {insertRefreshTokenDetails.Email} \n");
                    }

                    return email;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting refresh token details in method InsertRefreshTokenDetailsAsync email : {insertRefreshTokenDetails.Email} : {ex.Message} \n");
                throw;
            }

        }

        //... Method for task schedular to delete token details after per 1 second...//
        public async Task SetTokenSchedularAsync()
        {
            try
            {
                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await dbConnection.OpenAsync();
                    await dbConnection.ExecuteAsync(
                        "stp_SetTokenSchedular",
                        commandType: CommandType.StoredProcedure
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while deleting data for both tokens in method SetTokenSchedularAsync : {ex.Message} \n");
                throw;
            }
        }
    }
}
