using Dapper;
using ExternalService.Interfaces;
using FruitStoreModels.ApplicationUser;
using FruitStoreModels.Auth;
using FruitStoreRepositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;

namespace FruitStoreRepositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserRepository> _logger;
        private readonly IEmailService _emailService;

        public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger, IEmailService emailService)
        {
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }
        //... Method for reset password by email...//
        public async Task<OperationResult<string>> ResetPasswordAsync(ResetPassword model)
        {
            try
            {
                _logger.LogInformation($"Attempting to reset password in method ResetPassword starts for user : {model.UserId} \n");

                OperationResult<string> result = new OperationResult<string>();

                int currentUserId = Convert.ToInt32(model.UserId);

                var user = await GetUserByUserIdAsync(currentUserId);

                if (user != null)
                {
                    _logger.LogInformation($"Verifying user token for user : {user.UserName} \n");

                    var codeDetails = await GetCodeDetailsByUserIdAsync(user.UserId);

                    if (codeDetails != null)
                    {
                        var decodeCode = HttpUtility.UrlDecode(codeDetails.Code);

                        if (decodeCode == model.Code)
                        {
                            _logger.LogInformation($"Verifying user token completed for user : {user.UserName} \n");

                            _logger.LogInformation($"Hashing new password for user : {user.UserName} \n");

                            string uPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                            _logger.LogInformation($"Resetting password for user : {user.UserName} \n");

                            int updatepasswordStatus = await UpdatePasswordByMailAsync(user.Email, uPassword);

                            if (updatepasswordStatus == 0)
                            {
                                result.Success = false;

                                result.Message = "There is some problem. We are not able to reset your password, Please contact to administrator.";
                            }
                            else
                            {
                                result.Success = true;

                                result.Message = "Your password has been updated successfully.";

                            }
                        }
                        else
                        {
                            _logger.LogError($"User token verification failed for user : {user.UserName} \n");

                            result.Success = false;

                            result.Message = "Your password reset link has been expired, Please generate a different link and try again.";
                        }
                    }
                }
                else
                {
                    _logger.LogError("User not found \n");

                    result.Success = false;

                    result.Message = "Invalid user details";

                }
                _logger.LogInformation($"Attempting to reset password in method ResetPassword completed successfully for user: {model.UserId} \n");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred during password reset process in method ResetPassword for user with Id : {model.UserId} : {ex.Message} \n");

                throw;
            }
        }

        //... Method for forgot password by email...//
        public async Task<OperationResult<string>> ForgotPaswordEmailAsync(AddForgotPasswordDetails forgotPassDetails)
        {
            try
            {
                _logger.LogInformation($"Starting to send email to user in method ForgotPaswordEmail starts for email : {forgotPassDetails.Email} \n");

                OperationResult<string> result = new OperationResult<string>();

                var userDetails = await GetUserByEmailAsync(forgotPassDetails.Email);

                if ((userDetails == null) || (forgotPassDetails.UserRole != userDetails.UserRole))
                {
                    _logger.LogError($"User not found in the system in method ForgotPaswordEmail for email : {forgotPassDetails.Email} \n");

                    result.Message = "We are not able to find your email in the system, Please try again.";

                    result.Success = false;
                }
                else
                {
                    var code = GeneratePasswordResetToken();

                    if (code == null)
                    {
                        _logger.LogError($"Problem occurred while to send email to user in method ForgotPaswordEmail : {forgotPassDetails.Email} \n");

                        result.Message = "Problem while forgot password..please try again.";

                        result.Success = false;

                    }
                    else
                    {
                        _logger.LogInformation($"Constructing password reset URL for user : {userDetails.UserName} \n");

                        var url = _configuration["ApplicationURL:PasswordResetURL"];

                        var callbackUrl = url + "?userId=" + userDetails.UserId + "&code=" + HttpUtility.UrlEncode(code);

                        _logger.LogInformation($"Sending password reset email to user : {userDetails.UserName} \n");

                        try
                        {
                            await _emailService.SendEmailAsync(forgotPassDetails.Email, userDetails.UserName, HtmlEncoder.Default.Encode(callbackUrl), "Fruitables password reset link", "ForgotPassword");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"An error occurred while sending the email for password reset : {ex.Message} \n");
                            result.Message = "Service is temporarily unavailable..try after sometime";
                            result.Success = false;
                            throw;
                        }
                        ForgotPasswordDetails insertForgotPasswordDetails = new()
                        {
                            UserName = userDetails.UserName,
                            Code = HttpUtility.UrlEncode(code),
                            UserId = userDetails.UserId,
                            Email = userDetails.Email
                        };

                        var forgotPasswordDetails = await InsertForgetPasswordAsync(insertForgotPasswordDetails);

                        if (forgotPasswordDetails != 0)
                        {
                            _logger.LogInformation($"Forgot password details inserted succesffuly in DB in method ForgotPaswordEmail : {forgotPassDetails.Email} \n");
                        }
                        else
                        {
                            result.Message = "Problem while forgot password..please try again.";

                            result.Success = false;

                            _logger.LogError($"Forgot password details insertion failed in DB in method ForgotPaswordEmail : {forgotPassDetails.Email} \n");
                        }
                        _logger.LogInformation($"Password reset email sent successfully to user : {userDetails.UserName} \n");

                        result.Message = "Password reset email sent successfully..Check your email";

                        result.Success = true;

                        _logger.LogInformation($"Send email to user in method ForgotPaswordEmail completed successfully for email : {forgotPassDetails.Email} \n");
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred during send email to user in method ForgotPaswordEmail for email : {forgotPassDetails.Email} : {ex.Message} \n");

                throw;
            }

        }

        //... Method for get user details by mail...//
        public async Task<UserDetails> GetUserByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Email retrieval by mail starts in method GetUserByEmailAsync starts for email : {email} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new
                    {
                        Email = email
                    };

                    _logger.LogInformation("Executing stored procedure stp_GetUserByEmail \n");

                    var user = await dbConnection.QueryFirstOrDefaultAsync<UserDetails>(
                        "stp_GetUserByEmail",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (user != null)
                    {
                        _logger.LogInformation($"Email retrieval by mail from DB successful in method GetUserByEmailAsync for email : {email} \n");
                    }
                    else
                    {
                        _logger.LogError($"Email retrieval by mail from DB failed in method GetUserByEmailAsync for email : {email} \n");
                    }
                    return user;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while email retrieval by mail in method GetUserByEmailAsync for email : {email} : {ex.Message} \n");

                throw;
            }

        }

        //... Method for get user details by name...//
        public async Task<UserDetails> GetUserByNameAsync(string UserName)
        {
            try
            {
                _logger.LogInformation($"User retrieval by name starts in method GetUserByNameAsync for username : {UserName} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new
                    {
                        UserName = UserName
                    };

                    _logger.LogInformation("Executing stored procedure stp_GetUserByUserName \n");

                    var users = await dbConnection.QueryFirstOrDefaultAsync<UserDetails>(
                        "stp_GetUserByUserName",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (users != null)
                    {
                        _logger.LogInformation($"User retrieval by name attempt from DB successfull in method GetUserByNameAsyc for username : {UserName} \n");
                    }
                    else
                    {
                        _logger.LogError($"User retrieval by name attempt from the DB failed in method GetUserByNameAsync for username : {UserName} \n");
                    }

                    return users;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while user retrieval by name  attempt in method GetUserByNameAsync for username : {UserName} : {ex.Message} \n");

                throw;
            }
        }

        //...Method for inserting new user into database...// 
        public async Task<int> InsertUserAsync(RegisterUser registerUser)
        {
            try
            {
                _logger.LogInformation($"User registration: Inserting user data in method InsertUserAsync starts for username : {registerUser?.UserName}, email : {registerUser?.Email} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection closed successfully \n");

                    var parameters = new
                    {
                        UserName = registerUser.UserName,
                        Email = registerUser.Email,
                        UserPassword = registerUser.UserPassword,
                        UserRole = registerUser.UserRole
                    };

                    _logger.LogInformation("Executing stored procedure stp_SetUserData \n");

                    var userId = await dbConnection.ExecuteScalarAsync<int>(
                        "stp_SetUserData",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");


                    if (userId != 0)
                    {
                        _logger.LogInformation($"User registration: inserting user data in database in method InsertUserAsync completed successfully for username : {registerUser?.UserName}, email : {registerUser?.Email} \n");
                        try
                        {
                            await _emailService.SendPromotionalEmailAsync(registerUser.Email, registerUser.UserName, "Welcome to fruitables");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"An error occurred while sending promotional email : {ex.Message} \n");
                            throw;
                        }
                    }
                    else
                    {
                        _logger.LogError($"User registration: inserting user data in database in method InsertUserAsync failed for username : {registerUser?.UserName}, email : {registerUser?.Email} \n");
                    }

                    return userId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while user registration: inserting user data in method InsertUserAsync for username : {registerUser?.UserName}, email : {registerUser?.Email} :{ex.Message} \n");
                throw;
            }

        }

        //...Method for update password by mail..// 
        public async Task<int> UpdatePasswordByMailAsync(string email, string encrytedPassword)
        {
            try
            {
                _logger.LogInformation($"Update password attempt in method UpdatePasswordByMailAsync starts for user: {email} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@Email", email);
                    parameters.Add("@UpdatedPassword", encrytedPassword);

                    _logger.LogInformation("Executing stored procedure stp_SetResetPasswordByMail \n");

                    int deleteStatus = await dbConnection.ExecuteAsync(
                         "stp_SetResetPasswordByMail",
                         parameters,
                         commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (deleteStatus != 0)
                    {
                        _logger.LogInformation($"Update password completed successfully in DB  in method UpdatePasswordByMailAsync for user : {email} \n");
                    }
                    else
                    {
                        _logger.LogError($"Falied to update password in DB in method UpdatePasswordByMailAsync for user : {email} \n");
                    }

                    return deleteStatus;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error update password in method UpdatePasswordByMailAsync for user: {email} : {ex.Message} \n");
                throw;
            }
        }

        //..Method for get user details by it's Id..//
        public async Task<UserDetails> GetUserByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"User retrieval by Id starts in method GetUserByUserIdAsync for userId: {userId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new
                    {
                        UserId = userId
                    };

                    _logger.LogInformation("Executing stored procedure stp_GetUserByUserId \n");

                    var users = await dbConnection.QueryFirstOrDefaultAsync<UserDetails>(
                        "stp_GetUserByUserId",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (users != null)
                    {
                        _logger.LogInformation($"User retrieval by Id from database successfull in method GetUserByUserIdAsync for userId : {userId} \n");
                    }
                    else
                    {
                        _logger.LogError($"User retrieval by Id from the database failed in method GetUserByUserIdAsync for userId : {userId} \n");
                    }

                    return users;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while user retrieval by Id in method GetUserByUserIdAsync for for userId: {userId} : {ex.Message} \n");

                throw;
            }
        }

        //..Method for insert forgot password details..//
        public async Task<int> InsertForgetPasswordAsync(ForgotPasswordDetails forgotPassword)
        {
            try
            {
                _logger.LogInformation($"Inserting forgot passoword data in method InsertForgetPasswordAsync starts for username : {forgotPassword.UserName} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection closed successfully \n");

                    var parameters = new
                    {
                        @UserName = forgotPassword.UserName,
                        @Code = forgotPassword.Code,
                        @UserId = forgotPassword.UserId,
                        @Email = forgotPassword.Email,
                    };

                    _logger.LogInformation("Executing stored procedure stp_SetForgotPasswordDetails \n");

                    var passId = await dbConnection.ExecuteScalarAsync<int>(
                        "stp_SetForgotPasswordDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");


                    if (passId != 0)
                    {
                        _logger.LogInformation($"Inserting forgot passoword data in DB sucessfull in method InsertForgetPasswordAsync for username : {forgotPassword.UserName} \n");
                    }
                    else
                    {
                        _logger.LogError($"Inserting forgot passoword data in DB failed in method InsertForgetPasswordAsync for username : {forgotPassword.UserName} \n");

                    }

                    return passId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while inserting forgot passoword data failed in method InsertForgetPasswordAsync for username : {forgotPassword.UserName} : {ex.Message}  \n");
                throw;
            }
        }

        //..Method for get code details by user Id..//
        public async Task<ForgotPasswordDetails> GetCodeDetailsByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Code details retrieval by Id starts in method GetCodeDetailsByUserIdAsync for userId: {userId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new
                    {
                        UserId = userId
                    };

                    _logger.LogInformation("Executing stored procedure stp_GetCodeDetailsByUserId \n");

                    var codeDetails = await dbConnection.QueryFirstOrDefaultAsync<ForgotPasswordDetails>(
                        "stp_GetCodeDetailsByUserId",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (codeDetails != null)
                    {
                        _logger.LogInformation($"Code details retrieval by Id from database successfull in method GetCodeDetailsByUserIdAsync for userId : {userId} \n");
                    }
                    else
                    {
                        _logger.LogError($"Code details retrieval by Id from the database not found in method GetCodeDetailsByUserIdAsync for userId : {userId} \n");
                    }

                    return codeDetails;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while code details retrieval by Id in method GetCodeDetailsByUserIdAsync for for userId: {userId} : {ex.Message} \n");

                throw;
            }
        }

        //..Method for get code details by user Id..//
        public async Task<OTPDetails> GetOTPDetailsByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"OTP details retrieval by Id starts in method GetOTPDetailsByUserIdAsync for userId: {userId} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new
                    {
                        UserId = userId
                    };

                    _logger.LogInformation("Executing stored procedure stp_GetOTPDetailsByUserId \n");

                    var OTPDetails = await dbConnection.QueryFirstOrDefaultAsync<OTPDetails>(
                        "stp_GetOTPDetailsByUserId",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");

                    if (OTPDetails != null)
                    {
                        _logger.LogInformation($"OTP details retrieval by Id from database successfull in method GetOTPDetailsByUserIdAsync for userId : {userId} \n");
                    }
                    else
                    {
                        _logger.LogError($"OTP details retrieval by Id from the database not found in method GetOTPDetailsByUserIdAsync for userId : {userId} \n");
                    }

                    return OTPDetails;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while OTP details retrieval by Id in method GetOTPDetailsByUserIdAsync for for userId: {userId} : {ex.Message} \n");

                throw;
            }
        }

        //... Method for send OTP by email...//
        public async Task<OperationResult<string>> SendOTPByEmailAsync(AddOTPDetails OTPDetails)
        {
            try
            {
                _logger.LogInformation($"Starting to send OTP to user in method SendOTPByEmailAsync starts for email : {OTPDetails.Email} \n");

                OperationResult<string> result = new OperationResult<string>();

                var userDetails = await GetUserByEmailAsync(OTPDetails.Email);

                if ((userDetails == null) || (OTPDetails.UserRole != userDetails.UserRole))
                {
                    _logger.LogError($"User not found in the system in method SendOTPByEmailAsync for email : {OTPDetails.Email} \n");

                    result.Message = "We are not able to find your email in the system, Please try again.";

                    result.Success = false;
                }
                else
                {
                    Random random = new Random();

                    int otp = random.Next(1000, 10000);

                    if (otp == 0)
                    {
                        _logger.LogError($"Problem occurred while to send OTP to user in method SendOTPByEmailAsync : {OTPDetails.Email} \n");

                        result.Message = "Problem while send OTP..please try again.";

                        result.Success = false;
                    }
                    else
                    {
                        try
                        {
                            await _emailService.SendOTPByEmailAsync(OTPDetails.Email, userDetails.UserName, "One time password from Fruitables", otp);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"An error occurred while sending the OTP by email : {ex.Message} \n");
                            result.Message = "Service is temporarily unavailable..try after sometime";
                            result.Success = false;
                            throw;
                        }
                        InsertOTPDetails insertOTPDetails = new()
                        {
                            UserName = userDetails.UserName,
                            OTP = otp,
                            UserId = userDetails.UserId,
                            Email = userDetails.Email
                        };

                        var insertedOTPDetails = await InsertOTPAsync(insertOTPDetails);

                        if (insertedOTPDetails != 0)
                        {
                            _logger.LogInformation($"OTP details inserted succesffuly in DB in method SendOTPByEmailAsync : {OTPDetails.Email} \n");
                        }
                        else
                        {
                            result.Message = "Problem while OTP sending..please try again.";

                            result.Success = false;

                            _logger.LogError($"OTP details insertion failed in DB in method SendOTPByEmailAsync : {OTPDetails.Email} \n");
                        }
                        _logger.LogInformation($"OTP sent successfully to user : {userDetails.UserName} \n");

                        result.Message = "OTP sent successfully..Check your email";

                        result.Success = true;

                        _logger.LogInformation($"Send OTP to user in method SendOTPByEmailAsync completed successfully for email : {OTPDetails.Email} \n");
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred during send OTP to user in method SendOTPByEmailAsync for email : {OTPDetails.Email} : {ex.Message} \n");

                throw;
            }
        }

        //..Method for insert OTP details..//
        public async Task<int> InsertOTPAsync(InsertOTPDetails OTPDetails)
        {
            try
            {
                _logger.LogInformation($"Inserting OTP data in method InsertOTPAsync starts for username : {OTPDetails.UserName} \n");

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection closed successfully \n");

                    var parameters = new
                    {
                        @UserName = OTPDetails.UserName,
                        @OTP = OTPDetails.OTP,
                        @UserId = OTPDetails.UserId,
                        @Email = OTPDetails.Email,
                    };

                    _logger.LogInformation("Executing stored procedure stp_SetOTPDetails \n");

                    var OTPId = await dbConnection.ExecuteScalarAsync<int>(
                        "stp_SetOTPDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation("Stored procedure completed successfully \n");


                    if (OTPId != 0)
                    {
                        _logger.LogInformation($"Inserting OTP data in DB sucessfull in method InsertOTPAsync for username : {OTPDetails.UserName} \n");
                    }
                    else
                    {
                        _logger.LogError($"Inserting OTP data in DB failed in method InsertOTPAsync for username : {OTPDetails.UserName} \n");

                    }

                    return OTPId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while inserting OTP data failed in method InsertOTPAsync for username : {OTPDetails.UserName} : {ex.Message}  \n");
                throw;
            }
        }

        // ..Method for confirm OTP details..//
        public async Task<OperationResult<string>> ConfirmOTPAsync(ConfirmOTP confirmOTP)
        {
            try
            {
                _logger.LogInformation($"Confirming OTP starts in method ConfirmOTPAsync for email : {confirmOTP.Email}  \n");

                OperationResult<string> result = new OperationResult<string>();

                var userDetails = await GetUserByEmailAsync(confirmOTP.Email);

                if (userDetails == null)
                {
                    _logger.LogError($"User not found in the system in method ConfirmOTPAsync for email : {confirmOTP.Email} \n");

                    result.Message = "We are not able to find your email in the system, Please try again.";

                    result.Success = false;
                }
                else
                {
                    var OTPDetails = await GetOTPDetailsByUserIdAsync(userDetails.UserId);

                    if (OTPDetails == null)
                    {
                        _logger.LogError($"Confirming OTP completed in method ConfirmOTPAsync for email : {confirmOTP.Email} \n");

                        result.Message = "OTP not found..";

                        result.Success = false;

                    }
                    else
                    {
                        if (OTPDetails.OTP == confirmOTP.OTP)
                        {
                            _logger.LogInformation($"Problem occurred while to fetching OTP details in method ConfirmOTPAsync invalid OTP : {confirmOTP.Email} \n");

                            result.Message = "OTP validation sucessfull.";

                            result.Success = true;

                        }
                        else
                        {
                            _logger.LogError($"Problem occurred while to fetching OTP details in method ConfirmOTPAsync invalid OTP : {confirmOTP.Email} \n");

                            result.Message = "Invalid OTP try again.";

                            result.Success = false;
                        }
                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while OTP confirming attempt in method ConfirmOTPAsync email : {confirmOTP.Email} : {ex.Message} \n");

                throw;
            }
        }

        //..Private method for generate password reset token..//
        private string GeneratePasswordResetToken()
        {
            try
            {
                _logger.LogInformation($"Processing generate password reset token starts in method GeneratePasswordResetToken \n");

                int tokenLength = 10;

                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%^&*+_0123456789";

                StringBuilder tokenBuilder = new StringBuilder();

                _logger.LogInformation("Using random function to create token \n");

                Random random = new Random();

                for (int i = 0; i < tokenLength; i++)
                {

                    int randomIndex = random.Next(chars.Length);
                    tokenBuilder.Append(chars[randomIndex]);
                }
                string token = tokenBuilder.ToString();

                if (token != null)
                {
                    _logger.LogInformation($"Processing generate password reset token completed in method GeneratePasswordResetToken : {token} \n");
                }
                else
                {
                    _logger.LogError($"Processing generate password reset token failed in method GeneratePasswordResetToken \n");
                }

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while processing generate password reset token in method GeneratePasswordResetToken : {ex.Message} \n");
                throw;
            }

        }
    }
}

