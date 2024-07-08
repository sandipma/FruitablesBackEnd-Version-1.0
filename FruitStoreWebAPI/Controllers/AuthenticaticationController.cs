using CommonHelperUtility;
using FruitStoreModels.ApplicationUser;
using FruitStoreRepositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FruitStoreModels.Response;
using FruitStoreRepositories.InterFaces;
using FruitStoreModels.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ExternalService.Interfaces;
using System.Data.SqlClient;

namespace FruitStoreWebAPI.Controllers
{
    /// <summary>
    /// Represents a class that handles authentication operations.
    /// </summary>
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticaticationController : ControllerBase
    {

        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<AuthenticaticationController> _logger;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Represents a class that handles authentication operations.
        /// </summary>

        public AuthenticaticationController(IUserRepository userRepository, ILogger<AuthenticaticationController> logger,
            ITokenRepository tokenRepository,
            IJwtService jwtService)
        {
            _userRepository = userRepository;
            _logger = logger;
            _tokenRepository = tokenRepository;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Registers a new user and creates a JWT token for authentication.
        /// </summary>
        /// <param name="registerUser">User registration details.</param>
        /// <returns>Returns a response indicating the result of the registration process.</returns>
        /// <response code="201">Returns a Created response if the registration is successful.</response>
        /// <response code="400">Returns a Bad Request response if the provided registration details are invalid.</response>
        /// <response code="409">Returns a Conflict response if there is a conflict with the provided user information.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during registration.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     POST /api/authenticatication/register-user
        ///     {
        ///         "UserName": "exampleuser",
        ///         "Email": "example@example.com",
        ///         "UserPassword": "examplepassword",
        ///         "UserRole": "User"
        ///     }
        /// 
        /// </example>
        [HttpPost("register-user")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUser registerUser)
        {
            _logger.LogInformation("Validation started in controller action method RegisterUser \n");

            _logger.LogInformation("User/Admin name validation started \n");

            if (string.IsNullOrEmpty(registerUser.UserName))
            {
                _logger.LogError("User/admin name is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Name is required"));
            }

            else if (registerUser.UserName.Length > 50)
            {
                _logger.LogError("User/admin name must be at most 50 characters long \n");
                return StatusCode(400, new ApiResponse<string>(400, "Name must be at most 50 characters long."));
            }
            _logger.LogInformation("User/Admin name validation passed.\n");

            _logger.LogInformation("Email validation started \n");

            if (string.IsNullOrEmpty(registerUser.Email))
            {
                _logger.LogError("Email is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email is required."));
            }

            else if (!UserValidator.IsValidEmail(registerUser.Email))
            {
                _logger.LogError("Invalid email format \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid email format..kindly try with proper email"));
            }
            else if (registerUser.Email.Length > 100)
            {
                _logger.LogError("Email must be at most 100 characters long \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email must be at most 100 characters long."));
            }

            _logger.LogInformation("Email validation passed \n");

            _logger.LogInformation("User/Admin password validation started \n");

            if (string.IsNullOrEmpty(registerUser.UserPassword))
            {
                _logger.LogError("User/ admin password is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Password is required."));
            }

            else if (registerUser.UserPassword.Length < 6 || registerUser.UserPassword.Length > 255)
            {
                _logger.LogError("User/admin Password must be at least 6 characters long and at most 255 characters long \n");
                return StatusCode(400, new ApiResponse<string>(400, "Password must be at least 6 characters long and at most 255 characters long."));
            }

            _logger.LogInformation("User/Admin Password validation passed \n");

            _logger.LogInformation("Role validation started \n");

            if (string.IsNullOrEmpty(registerUser.UserRole))
            {
                _logger.LogError("Role is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Your role is not specified..try again"));
            }
            else if (registerUser.UserRole != UserRoles.Admin && registerUser.UserRole != UserRoles.User)
            {
                _logger.LogError("Role is invalid \n");
                return StatusCode(400, new ApiResponse<string>(400, "Your role is invalid..try again"));
            }
            _logger.LogInformation("Role validation completed \n");

            _logger.LogInformation("Validation completed in controller action method RegisterUser \n");

            try
            {
                _logger.LogInformation($"User/admin registration starts in method RegisterUser for name : {registerUser.UserName} \n");

                _logger.LogInformation($"User/admin password hashing starts name : {registerUser.UserName} \n");

                registerUser.UserPassword = BCrypt.Net.BCrypt.HashPassword(registerUser.UserPassword);

                _logger.LogInformation($"User/admin password hashing completed name : {registerUser.UserName} \n");

                int userId = await _userRepository.InsertUserAsync(registerUser);

                if (userId == 0)
                {
                    _logger.LogError($"Registration failed for user/admin in method RegisterUser for name : {registerUser.UserName} \n");
                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                }
                _logger.LogInformation($"User/admin registration successful in method RegisterUser for name : {registerUser.UserName} \n");

                return StatusCode(201, new ApiResponse<string>(201, $"Welcome, {registerUser.UserName}! Your registration was successful."));
            }
            // Catch exception here //
            catch (SqlException ex)
            {
                string errorMessage = string.Empty;
                if (ex is SqlException sqlException && sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("Name cannot be null."))
                    {
                        _logger.LogError($"Registration failed for user/admin in method RegisterUser for name : {registerUser.UserName} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Name is required"));
                    }
                    else if (sqlException.Message.Contains("Email cannot be null."))
                    {
                        _logger.LogError($"Registration failed for user/admin in method RegisterUser for user/admin name : {registerUser.UserName} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Email cannot be null"));
                    }

                    else if (sqlException.Message.Contains("Name is already in use."))
                    {
                        errorMessage = "Name is already taken";
                        _logger.LogError($"Registration failed for user/admin in method RegisterUser for user/admin name : {registerUser.UserName} : {sqlException.Message} \n");
                        return StatusCode(409, new ApiResponse<string>(400, errorMessage));
                    }

                    else if (sqlException.Message.Contains("Email is already in use."))
                    {
                        errorMessage = "Email is already taken";
                        _logger.LogError($"Registration failed for user/admin in method RegisterUser for user/admin name : {registerUser.UserName} : {sqlException.Message} \n");
                        return StatusCode(409, new ApiResponse<string>(400, errorMessage));
                    }

                    else if (sqlException.Message.Contains("More than 3 administrators are not allowed. Please contact the product owner for assistance."))
                    {
                        errorMessage = "More than 3 administrators are not allowed. Please contact the product owner for assistance.";
                        _logger.LogError($"Registration failed for admin in method RegisterUser for admin name : {registerUser.UserName} : {sqlException.Message} \n");
                        return StatusCode(409, new ApiResponse<string>(400, errorMessage));

                    }
                    else if (sqlException.Message.Contains("Role cannot be null."))
                    {
                        _logger.LogError($"Registration failed for user/admin in method RegisterUser for name : {registerUser.UserName} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Please login with proper credentials"));
                    }
                             
                }
                _logger.LogError($"SQL Error during register a user/admin  in method RegisterUser for name : {registerUser.UserName} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error registering user/admin in method RegisterUser for name : {registerUser.UserName} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
            }
        }


        /// <summary>
        /// Logs in a user and generates a JWT token for authentication.
        /// </summary>
        /// <param name="user">User login credentials.</param>
        /// <returns>Returns a response indicating the result of the login process.</returns>
        /// <response code="200">Returns an OK response with the generated JWT token if the login is successful.</response>
        /// <response code="400">Returns a Bad Request response if the provided credentials are invalid or missing.</response>
        /// <response code="404">Returns a Not Found response if the user does not exist.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during login.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     POST /api/authenticatication/login-user
        ///     {
        ///         "UserName": "exampleuser",
        ///         "Password": "examplepassword"
        ///     }
        /// 
        /// </example>
        [HttpPost("login-user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TokenDetails>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> LoginUser(LoginUser user)
        {
            _logger.LogInformation("Validation started in controller action method LoginUser \n");

            _logger.LogInformation("User/Admin Name validation started \n");

            if (string.IsNullOrEmpty(user.UserName))
            {
                _logger.LogError("User/admin name is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "Name is required"));
            }
            else if (user.UserName.Length > 50)
            {
                _logger.LogError("User/admin Name must be at most 50 characters long \n");

                return StatusCode(400, new ApiResponse<string>(400, "Name must be at most 50 characters long."));
            }
            _logger.LogInformation("User/Admin Name validation passed \n");

            _logger.LogInformation("User Password validation started \n");

            if (string.IsNullOrEmpty(user.Password))
            {
                _logger.LogError("User/admin Password is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Password is required."));
            }

            _logger.LogInformation("User/Admin Password validation passed \n");

            _logger.LogInformation("User/Admin UserOrAdmin validation started \n");

            if (string.IsNullOrEmpty(user.UserOrAdmin))
            {
                _logger.LogError("UserOrAdmin flag is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Kindly specify your role..can't be null"));
            }
            _logger.LogInformation("User/Admin UserOrAdmin validation passed \n");

            _logger.LogInformation("Validation completed in controller action method LoginUser \n");

            try
            {
                _logger.LogInformation($"Attempting to login user/admin in method LoginUser for name : {user.UserName} \n");

                TokenDetails details = new TokenDetails();

                var userDetails = await _userRepository.GetUserByNameAsync(user.UserName);

                if (userDetails != null)
                {
                    if ((user.UserOrAdmin != userDetails.UserRole) || (user.UserName != userDetails.UserName))
                    {
                        _logger.LogError($"Wrong details entered for user/admin : {user.UserName} \n");
                        if (user.UserOrAdmin == "admin")
                        {
                            return StatusCode(400, new ApiResponse<string>(400, "Admin not exists"));
                        }
                        return StatusCode(400, new ApiResponse<string>(400, "User not exists"));
                    }
                    _logger.LogInformation($"Details fetched successfull for user/admin : {user.UserName} \n");

                    if (!BCrypt.Net.BCrypt.Verify(user.Password, userDetails.UserPasswordHash))
                    {
                        _logger.LogError($"Wrong password entered for user/admin : {user.UserName} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Wrong password... Kindly try again"));
                    }
                    _logger.LogInformation($"Creating auth Claims to user/admin in method LoginUser : {user.UserName} \n");

                    var authClaims = new List<Claim>
                    {

                        new Claim(ClaimTypes.Name, userDetails.UserName),
                        new Claim(ClaimTypes.Email, userDetails.Email),
                        new Claim(ClaimTypes.Role,userDetails.UserRole)
                    };

                    _logger.LogInformation($"Completing auth Claims to user/admin in method LoginUser : {user.UserName} \n");

                    _logger.LogInformation($"Generating JWT token for user/admin : {user.UserName} \n");

                    var accessToken = _jwtService.GetToken(authClaims, userDetails.UserName, "A");

                    var refreshToken = _jwtService.GetToken(authClaims, userDetails.UserName, "R");

                    if (accessToken == null || refreshToken == null)
                    {
                        _logger.LogError($"Failed to generate JWT token for user/admin : {user.UserName} \n");
                        return StatusCode(500, new ApiResponse<string>(500, "Login failed..server error please check and try again"));
                    }
                    else
                    {
                        // Generate and validate access token
                        var accessTokenDetails = await _tokenRepository.GenerateJwtTokenDetailsAsync(userDetails, accessToken);

                        var refreshTokenDetails = await _tokenRepository.GenerateRefreshTokenDetailsAsync(userDetails, refreshToken);

                        if (accessTokenDetails == null || refreshTokenDetails == null)
                        {
                            _logger.LogError($"Failed to generate token for user/admin : {user.UserName} \n");
                            return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                        }

                        if (accessTokenDetails != null)
                        {
                            details.AccessTokenDetails = accessTokenDetails;
                        }

                        if (refreshTokenDetails != null)
                        {
                            details.RefreshTokenDetails = refreshTokenDetails;
                        }

                        _logger.LogInformation($"Login successfull for user/admin in method LoginUser : {user.UserName} \n");
                        return StatusCode(200, new ApiResponse<TokenDetails>(200, $"Welcome back, {user.UserName}! Your login was successful.", details));

                    }
                }
                return StatusCode(404, new ApiResponse<TokenDetails>(404, "User not exists"));
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException && sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("UserName cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to login failed for user/admin in method LoginUser : {user.UserName} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Name cannot be null"));
                    }
                    else if (sqlException.Message.Contains("User with the provided name does not exist.") && user.UserOrAdmin == "user")
                    {
                        _logger.LogError($"Attempting to login failed for user in method LoginUser : {user.UserName} : {sqlException.Message} \n");
                        return StatusCode(404, new ApiResponse<string>(404, "User not exists...Kindly register"));
                    }
                    else if (sqlException.Message.Contains("User with the provided name does not exist.") && user.UserOrAdmin == "admin")
                    {
                        _logger.LogError($"Attempting to login failed for admin in method LoginUser : {user.UserName} : {sqlException.Message} \n");
                        return StatusCode(404, new ApiResponse<string>(404, "Admin not exists...Kindly register"));
                    }
                    else if (sqlException.Message.Contains("Email cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to login failed for user/admin in method LoginUser : {user.UserName} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Email cannot be NULL"));
                    }
                    else if (sqlException.Message.Contains("AccessToken cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to login failed for user/admin in method LoginUser : {user.UserName} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                    else if (sqlException.Message.Contains("Access token time period cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to login failed for user/admin in method LoginUser : {user.UserName} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                    else if (sqlException.Message.Contains("RefreshToken cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to login failed for user/admin in method LoginUser : {user.UserName} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                    else if (sqlException.Message.Contains("Refresh token time period cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to login failed for user/admin in method LoginUser : {user.UserName} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }             
                    else
                    {
                        _logger.LogError($"SQL Error during login a user  in method LoginUser for user/admin name : {user.UserName} : {sqlException.Message} \n");
                        return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                    }
                }
                else
                {
                    _logger.LogError($"Error during login a user  in method LoginUser for user/admin name : {user.UserName} : {ex.Message} \n");
                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end... Server error"));
                }
            }
        }


        /// <summary>
        /// Logs in a user using Google authentication and generates a JWT token for authentication.
        /// </summary>
        /// <param name="mail">User login credentials with email.</param>
        /// <returns>Returns a response indicating the result of the login process.</returns>
        /// <response code="200">Returns an OK response with the generated JWT token if the login is successful.</response>
        /// <response code="400">Returns a Bad Request response if the provided credentials are invalid.</response>
        /// <response code="404">Returns a Not Found response if the user does not exist.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during login.</response>
        /// <example>
        /// Sample request:
        /// 
        ///     POST /api/authenticatication/google-login
        ///     {
        ///         "Email": "example@example.com"
        ///     }
        /// 
        /// </example>    
        [HttpPost("google-login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TokenDetails>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> GoogleLogin(EmailData mail)
        {
            _logger.LogInformation("Validation started in controller action method GoogleLogin \n");

            _logger.LogInformation("Email validation starts \n");

            if (string.IsNullOrEmpty(mail.Email))
            {
                _logger.LogError("Email is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email is required."));
            }
            else if (!UserValidator.IsValidEmail(mail.Email))
            {
                _logger.LogError("Invalid email format \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid email format"));
            }
            else if (mail.Email.Length > 100)
            {
                _logger.LogError("Email must be at most 100 characters long \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email must be at most 100 characters long."));
            }
            _logger.LogInformation("Email validation passed.");

            _logger.LogInformation("Validation completed in controller action method GoogleLogin \n");

            try
            {
                _logger.LogInformation($"Attempting to login user in method GoogleLogin : {mail.Email} \n");

                TokenDetails details = new TokenDetails();

                var userDetails = await _userRepository.GetUserByEmailAsync(mail.Email);

                if (userDetails != null)
                {
                    if (userDetails.UserRole == "admin")
                    {
                        _logger.LogError($"Wrong details entered for user/admin : {userDetails.UserName} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "You do not have permissions to use Google login"));
                    }
                    _logger.LogInformation($"Details fetched successfull for user : {mail.Email} \n");

                    _logger.LogInformation($"Creating auth Claims to user in method LoginUser : {mail.Email} \n");

                    var authClaims = new List<Claim>
                    {

                        new Claim(ClaimTypes.Name, userDetails.UserName),
                        new Claim(ClaimTypes.Email, userDetails.Email),
                        new Claim(ClaimTypes.Role,userDetails.UserRole)
                    };

                    _logger.LogInformation($"Completing auth Claims to user in method GoogleLogin : {mail.Email} \n");

                    _logger.LogInformation($"Generating JWT token for user : {mail.Email} \n");

                    var accessToken = _jwtService.GetToken(authClaims, userDetails.UserName, "A");

                    var refreshToken = _jwtService.GetToken(authClaims, userDetails.UserName, "R");

                    if (accessToken == null || refreshToken == null)
                    {
                        _logger.LogError($"Failed to generate JWT token for user : {mail.Email} \n");
                        return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end... Server error"));
                    }
                    else
                    {
                        var accessTokenDetails = await _tokenRepository.GenerateJwtTokenDetailsAsync(userDetails, accessToken);

                        var refreshTokenDetails = await _tokenRepository.GenerateRefreshTokenDetailsAsync(userDetails, refreshToken);

                        if (accessTokenDetails == null || refreshTokenDetails == null)
                        {
                            _logger.LogError($"Failed to generate token for user : {mail.Email} \n");
                            return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end... Server error"));
                        }

                        if (accessTokenDetails != null)
                        {
                            details.AccessTokenDetails = accessTokenDetails;
                        }

                        if (refreshTokenDetails != null)
                        {
                            details.RefreshTokenDetails = refreshTokenDetails;
                        }

                        _logger.LogInformation($"Login successfull for user in method GoogleLogin : {mail.Email} \n");
                        return StatusCode(200, new ApiResponse<TokenDetails>(200, $"Welcome back, {userDetails.UserName}! Your login was successful.", details));
                    }
                }
                _logger.LogError($"Attempting to login failed for user in method GoogleLogin : {mail.Email} \n");
                return StatusCode(404, new ApiResponse<string>(404, "User email address not exists in our sysytem..kindly register"));
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException && sqlException.Number == 50000)
                {
                    if (sqlException.Message.Contains("UserName cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to login failed for user in method GoogleLogin : {mail.Email} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "UserName cannot be null"));
                    }
                    else if (sqlException.Message.Contains("User with the provided UserName does not exist."))
                    {
                        _logger.LogError($"Attempting to login failed for user in method GoogleLogin : {mail.Email} : {sqlException.Message} \n");
                        return StatusCode(404, new ApiResponse<string>(404, "User not exists...Kindly register"));
                    }
                    else if (sqlException.Message.Contains("Email cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to login failed for user in method GoogleLogin : {mail.Email} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Email cannot be NULL or empty."));
                    }
                    else if (sqlException.Message.Contains("AccessToken cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to login failed for user in method GoogleLogin : {mail.Email} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                    else if (sqlException.Message.Contains("Access token time period cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to login failed for user in method GoogleLogin : {mail.Email} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                    else if (sqlException.Message.Contains("RefreshToken cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to login failed for user in method GoogleLogin : {mail.Email} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                    else if (sqlException.Message.Contains("Refresh token time period cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to login failed for user in method GoogleLogin : {mail.Email} : {sqlException.Message} \n");
                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                    else
                    {
                        _logger.LogError($"SQL Error during login a user in method GoogleLogin for : {mail.Email} : {sqlException.Message} \n");
                        return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end...Server error"));
                    }
                }
                else
                {
                    _logger.LogError($"Error during login a user in method GoogleLogin for : {mail.Email} : {ex.Message} \n");
                    return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end... Server error"));
                }
            }

        }


        /// <summary>
        /// Logs out a user by deleting their token details.
        /// </summary>
        /// <param name="mail">User email for logout.</param>
        /// <returns>Returns a response indicating the result of the logout process.</returns>
        /// <response code="200">Returns an OK response if the logout is successful.</response>
        /// <response code="400">Returns a Bad Request response if the provided email is invalid or if there is a SQL exception.</response>
        /// <response code="404">Returns a Not Found response if the result does not exist.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs during logout.</response>
        /// <example>
        /// Sample request:
        ///
        ///     DELETE /api/authenticatication/logout
        ///     {
        ///         "Email": "example@example.com"
        ///     }
        /// 
        /// </example>
        [HttpDelete("logout")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]  
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> Logout(EmailData mail)
        {
            _logger.LogInformation("Validation started in controller action method Logout \n");

            _logger.LogInformation("Email validation starts \n");

            if (string.IsNullOrEmpty(mail.Email))
            {
                _logger.LogError("Email is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email is required."));
            }
            else if (!UserValidator.IsValidEmail(mail.Email))
            {
                _logger.LogError("Invalid email format \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid email format"));
            }
            else if (mail.Email.Length > 100)
            {
                _logger.LogError("Email must be at most 100 characters long \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email must be at most 100 characters long."));
            }
            _logger.LogInformation("Email validation passed \n");

            _logger.LogInformation("Validation completed in controller action method Logout \n");

            try
            {
                _logger.LogInformation($"Attempting to delete token details in method Logout for email : {mail.Email} \n");

                int result = await _tokenRepository.DeleteTokenDetailsOnLogoutByMailAsync(mail.Email);

                if (result != 0)
                {
                    _logger.LogInformation($"Logout successful for email : {mail.Email} \n");   
                }
                return StatusCode(200, new ApiResponse<string>(200, "Logout Successfully.."));
            }
            catch (SqlException sqlex)
            {
                if (sqlex.Number == 50000)
                {
                    if (sqlex.Message.Contains("Email cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to delete token details failed in method Logout for email : {mail.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Email can not null"));
                    }                         
                }
                _logger.LogError($"Sql exception error during logout in method Logout for Email : {mail.Email} : {sqlex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end... Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during logout in method Logout  for Email: {mail.Email} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end... Server error"));
            }
        }


        /// <summary>
        /// Sends a forgot password email to the specified email address.
        /// </summary>
        /// <param name="forgotPasswordDetails">The forgot password details to send the forgot password email to.</param>
        /// <returns>Returns a response indicating the result of sending the forgot password email.</returns>
        /// <response code="200">Returns a success response if the email was sent successfully.</response>
        /// <response code="400">Returns a Bad Request response if the provided email is invalid or if there is a problem with the request.</response>
        /// <response code="404">Returns a Not Found response if the email is not found in the system.</response>
        /// <response code="500">Returns an Internal server error response if an unexpected error occurs.</response>
        /// <response code="503">Returns a Service Unavailable response if the service is temporarily unavailable.</response>
        /// <example>
        /// Sample request:
        ///
        ///     POST /api/authenticatication/forgot-password
        ///     {
        ///         "Email": "example@example.com"
        ///     }
        ///</example>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> SendForgotPasswordEmail(AddForgotPasswordDetails forgotPasswordDetails)
        {
            _logger.LogInformation("Validation started in controller action method SendForgotPasswordEmail \n");

            _logger.LogInformation("Email validation starts \n");

            if (string.IsNullOrEmpty(forgotPasswordDetails.Email))
            {
                _logger.LogError("Email is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email is required."));
            }
            else if (!UserValidator.IsValidEmail(forgotPasswordDetails.Email))
            {
                _logger.LogError("Invalid email format \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid email format"));
            }
            else if (forgotPasswordDetails.Email.Length > 100)
            {
                _logger.LogError("Email must be at most 100 characters long \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email must be at most 100 characters long."));
            }
            _logger.LogInformation("Email validation passed \n");

            _logger.LogInformation("Role validation started \n");

            if (string.IsNullOrEmpty(forgotPasswordDetails.UserRole))
            {
                _logger.LogError("Invalid details for role \n");
                return StatusCode(400, new ApiResponse<string>(400, "Kindly specify your role..can't be null"));
            }
            _logger.LogInformation("Role UserOrAdmin validation passed \n");

            _logger.LogInformation("Validation passed in controller action method SendForgotPasswordEmail \n");

            try
            {
                _logger.LogInformation($"Sending forgot password by email starts in method SendForgotPasswordEmail for email : {forgotPasswordDetails.Email} \n");

                var result = await _userRepository.ForgotPaswordEmailAsync(forgotPasswordDetails);

                if (!result.Success && result.Message.Contains("We are not able to find your email in the system, Please try again."))
                {
                    _logger.LogError($"Sending forgot password by email failed in method SendForgotPasswordEmail for email : {forgotPasswordDetails.Email} : {result.Message} \n");

                    return StatusCode(404, new ApiResponse<string>(404, result.Message));
                }
                else if (!result.Success && result.Message.Contains("Problem while forgot password.., Please try again."))
                {
                    _logger.LogError($"Sending forgot password by email failed in method SendForgotPasswordEmail for email : {forgotPasswordDetails.Email} : {result.Message} \n");

                    return StatusCode(400, new ApiResponse<string>(400, result.Message));
                }
                else if (!result.Success && result.Message.Contains("Service is temporarily unavailable..try after sometime"))
                {
                    _logger.LogError($"Sending forgot password by email failed in method SendForgotPasswordEmail for email : {forgotPasswordDetails.Email} : {result.Message} \n");

                    return StatusCode(503, new ApiResponse<string>(503, result.Message));
                }
                else if (result.Success)
                {
                    _logger.LogInformation($"Sending forgot password by email completed in method SendForgotPasswordEmail for email : {forgotPasswordDetails.Email} : {result.Message} \n");
                    return StatusCode(200, new ApiResponse<string>(200, result.Message));
                }

                _logger.LogError($"Unknown error occurred in method SendForgotPasswordEmail for mail : {forgotPasswordDetails.Email} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end... Server error"));
            }
            catch (SqlException sqlex)
            {
                if (sqlex.Number == 50000)
                {
                    if (sqlex.Message.Contains("UserName cannot be null."))
                    {
                        _logger.LogError($"Sending forgot password by email failed in method SendForgotPasswordEmail for email : {forgotPasswordDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "name cannot be null."));
                    }
                    else if (sqlex.Message.Contains("Code cannot be null."))
                    {
                        _logger.LogError($"Sending forgot password by email failed in method SendForgotPasswordEmail for email : {forgotPasswordDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                    else if (sqlex.Message.Contains("User Id cannot be null."))
                    {
                        _logger.LogError($"Sending forgot password by email failed in method SendForgotPasswordEmail for email : {forgotPasswordDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                    else if (sqlex.Message.Contains("Email cannot be null."))
                    {
                        _logger.LogError($"Sending forgot password by email failed in method SendForgotPasswordEmail for email : {forgotPasswordDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Email cannot be null."));
                    }
                }
                _logger.LogError($"Sql exception error during forgot password by email failed in method SendForgotPasswordEmail for email : {forgotPasswordDetails.Email} : {sqlex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end... Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($" Error while sending forgot password by email in method SendForgotPasswordEmail mail : {forgotPasswordDetails.Email} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, $"Oops! Something went wrong on our end... Server error"));
            }
        }


        /// <summary>
        /// Resets the password for a user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     POST : /api/authenticatication/reset-password
        ///     {
        ///         "Password": "newpassword123",
        ///         "UserId": "exampleUserID",
        ///         "code": "exampleCode123"
        ///     }
        ///
        /// </remarks>
        /// <param name="model">The model containing password, UserId, and code.</param>
        /// <returns>Returns a response indicating the result of the password reset process.</returns>
        /// <response code="200">Returns a success response if the password is reset successfully.</response>
        /// <response code="400">Returns a Bad Request response if the input data is invalid or if there's an SQL exception.</response>
        /// <response code="500">Returns an Internal Server Error response if an unexpected error occurs.</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> ResetPassword(ResetPassword model)
        {
            _logger.LogInformation("Validation started in controller action in method ResetPassword \n");

            _logger.LogInformation("Validation starts for password \n");

            if (string.IsNullOrEmpty(model.Password))
            {
                _logger.LogError("Password is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "Password is required."));
            }
            else if (model.Password.Length < 6 || model.Password.Length > 255)
            {
                _logger.LogError("Password must be between 6 and 255 characters \n");

                return StatusCode(400, new ApiResponse<string>(400, "Password must be between 6 and 255 characters."));
            }
            _logger.LogInformation("Validation completed for password \n");

            _logger.LogInformation("Validation starts for User Id \n");

            if (string.IsNullOrEmpty(model.UserId))
            {
                _logger.LogError("UserId is required \n");

                return StatusCode(400, new ApiResponse<string>(400, "User can not be null."));
            }

            _logger.LogInformation("Validation completed for User Id \n");

            _logger.LogInformation("Validation starts for code \n");

            if (string.IsNullOrEmpty(model.Code))
            {
                _logger.LogError("Code is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
            }
            _logger.LogInformation("Validation completed for code \n");

            _logger.LogInformation("Validation completed in controller action in method ResetPassword \n");

            try
            {
                _logger.LogInformation($"Attempting to reset password in method ResetPassword for UserId : {model.UserId} \n");

                var result = await _userRepository.ResetPasswordAsync(model);

                if (result.Success)
                {
                    _logger.LogInformation($"Password reset successfully in method ResetPassword for UserId : {model.UserId} \n");
                    return StatusCode(200, new ApiResponse<string>(200, result.Message));
                }
                else
                {
                    _logger.LogError($"Password reset failed in method ResetPassword for UserId : {model.UserId} \n");
                    return StatusCode(500, new ApiResponse<string>(500, result.Message));
                }
            }
            catch (SqlException sqlex)
            {
                if (sqlex.Number == 50000)
                {
                    if (sqlex.Message.Contains("User Id cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to reset password failed in method ResetPassword for UserId : {model.UserId} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                    else if (sqlex.Message.Contains("User with the provided User Id does not exist."))
                    {
                        _logger.LogError($"Attempting to reset password failed in method ResetPassword for UserId : {model.UserId} : {sqlex.Message} \n");

                        return StatusCode(404, new ApiResponse<string>(404, "details not exists"));
                    }
                    else if (sqlex.Message.Contains("Email cannot be NULL or empty."))
                    {
                        _logger.LogError($"Attempting to reset password failed in method ResetPassword for UserId : {model.UserId} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                }
                _logger.LogError($"Sql exception error during reset password in method ResetPassword for UserId : {model.UserId} : {sqlex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end... Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred at reset password in method ResetPassword for UserId : {model.UserId} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, $"Oops! Something went wrong on our end... Server error"));
            }

        }


        /// <summary>
        /// Sends OTP details to the provided email address.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     POST : /api/authenticatication/send-OTP-details
        ///     {
        ///         "Email": "newpassword123@gg.com",
        ///         "UserRole": "user"
        ///     }
        ///
        /// </remarks>
        /// <param name="OTPDetails">The model containing the email address to send the OTP.</param>
        /// <returns>Returns a response indicating the result of sending the OTP.</returns>
        /// <response code="200">Returns a success response if the password is reset successfully.</response>
        /// <response code="400">Returns a Bad Request response if the input data is invalid</response>
        /// <response code="404">Returns a not found if data is not found</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="503">Returns service unavailabe.</response>
        [HttpPost("send-OTP-details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> SendOTPDetails(AddOTPDetails OTPDetails)
        {
            _logger.LogInformation("Validation started in controller action method SendOTPDetails \n");

            _logger.LogInformation("Email validation starts \n");

            if (string.IsNullOrEmpty(OTPDetails.Email))
            {
                _logger.LogError("Email is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email is required."));
            }
            else if (!UserValidator.IsValidEmail(OTPDetails.Email))
            {
                _logger.LogError("Invalid email format \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid email format"));
            }
            else if (OTPDetails.Email.Length > 100)
            {
                _logger.LogError("Email must be at most 100 characters long \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email must be at most 100 characters long."));
            }
            _logger.LogInformation("Email validation passed \n");

            _logger.LogInformation("Role validation started \n");

            if (string.IsNullOrEmpty(OTPDetails.UserRole))
            {
                _logger.LogError("Invalid details for role \n");
                return StatusCode(400, new ApiResponse<string>(400, "Kindly specify your role..can't be null"));
            }
            _logger.LogInformation("Role UserOrAdmin validation passed \n");

            _logger.LogInformation("Validation passed in controller action method SendOTPDetails \n");
            try
            {
                _logger.LogInformation($"Sending OTP by email starts in method SendOTPDetails for email : {OTPDetails.Email} \n");

                var result = await _userRepository.SendOTPByEmailAsync(OTPDetails);

                if (!result.Success && result.Message.Contains("We are not able to find your email in the system, Please try again."))
                {
                    _logger.LogError($"Sending OTP by email failed in method SendOTPDetails for email : {OTPDetails.Email} : {result.Message} \n");

                    return StatusCode(404, new ApiResponse<string>(404, result.Message));
                }
                else if (!result.Success && result.Message.Contains("Problem while sending OTP .., Please try again."))
                {
                    _logger.LogError($"Sending OTP by email failed in method SendOTPDetails for email : {OTPDetails.Email} : {result.Message} \n");

                    return StatusCode(400, new ApiResponse<string>(400, result.Message));
                }
                else if (!result.Success && result.Message.Contains("Service is temporarily unavailable..try after sometime"))
                {
                    _logger.LogError($"Sending OTP by email failed in method SendOTPDetails for email : {OTPDetails.Email} : {result.Message} \n");

                    return StatusCode(503, new ApiResponse<string>(503, result.Message));
                }
                else if (result.Success)
                {
                    _logger.LogInformation($"Sending OTP by email completed in method SendOTPDetails for email : {OTPDetails.Email} : {result.Message} \n");
                    return StatusCode(200, new ApiResponse<string>(200, result.Message));
                }

                _logger.LogError($"Unknown error occurred in method SendOTPDetails for mail : {OTPDetails} \n");

                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end... Server error"));
            }
            catch (SqlException sqlex)
            {
                if (sqlex.Number == 50000)
                {
                    if (sqlex.Message.Contains("UserName cannot be null."))
                    {
                        _logger.LogError($"Sending OTP by email failed in method SendOTPDetails for email : {OTPDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "name cannot be null."));
                    }
                    else if (sqlex.Message.Contains("OTP cannot be null."))
                    {
                        _logger.LogError($"Sending OTP by email failed in method SendOTPDetails for email : {OTPDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid OTP..try again"));
                    }
                    else if (sqlex.Message.Contains("User Id cannot be null."))
                    {
                        _logger.LogError($"Sending OTP by email failed in method SendOTPDetails for email : {OTPDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                    else if (sqlex.Message.Contains("Email cannot be null."))
                    {
                        _logger.LogError($"Sending OTP by email failed in method SendOTPDetails for email : {OTPDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Email cannot be null."));
                    }
                }
                _logger.LogError($"Sql exception error during OTP by email failed in method SendOTPDetails for email : {OTPDetails.Email} : {sqlex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end... Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($" Error while sending OTP by email failed in method SendOTPDetails mail : {OTPDetails} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, $"Oops! Something went wrong on our end... Server error"));
            }
        }


        /// <summary>
        /// Confirms OTP details for the provided email address.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     POST : /api/authenticatication/confirm-OTP-details
        ///     {
        ///         "Email": "newpassword123@gg.com",
        ///         "OTP": "1234"
        ///     }
        ///
        /// </remarks>
        /// <param name="OTPDetails">The model containing the email address to send the OTP.</param>
        /// <returns>Returns a response indicating the result of sending the OTP.</returns>
        /// <response code="200">Returns a success response if the password is reset successfully.</response>
        /// <response code="400">Returns a Bad Request response if the input data is invalid</response>     
        /// <response code="500">Internal server error.</response>
        [HttpPost("confirm-OTP-details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<string>))]
        public async Task<IActionResult> ConfirmOTPDetails(ConfirmOTP OTPDetails)
        {

            _logger.LogInformation("Validation started in controller action in method ConfirmOTPDetails \n");

            _logger.LogInformation("Validation starts for email \n");

            if (string.IsNullOrEmpty(OTPDetails.Email))
            {
                _logger.LogError("Email is required \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email is required."));
            }

            else if (!UserValidator.IsValidEmail(OTPDetails.Email))
            {
                _logger.LogError("Invalid email format \n");
                return StatusCode(400, new ApiResponse<string>(400, "Invalid email format..kindly try with proper email"));
            }
            else if (OTPDetails.Email.Length > 100)
            {
                _logger.LogError("Email must be at most 100 characters long \n");
                return StatusCode(400, new ApiResponse<string>(400, "Email must be at most 100 characters long."));
            }
            _logger.LogInformation("Validation Completed for email \n");

            _logger.LogInformation("Validation starts for OTP \n");

            if (OTPDetails.OTP == 0)
            {
                _logger.LogError("OTP can not be null \n");

                return StatusCode(400, new ApiResponse<string>(400, "OTP can not be null"));
            }
            else if (OTPDetails.OTP.ToString().Length < 4)
            {
                _logger.LogError("OTP must be atleast 4 digits \n");

                return StatusCode(400, new ApiResponse<string>(400, "OTP must be atleast 4 digits"));
            }
            _logger.LogInformation("Validation passed for OTP \n");

            _logger.LogInformation("Validation completed in controller action in method ConfirmOTPDetails \n");

            try
            {
                _logger.LogInformation($"Attempting to confirm OTP in method ConfirmOTPDetails for email : {OTPDetails.Email} \n");

                var result = await _userRepository.ConfirmOTPAsync(OTPDetails);

                if (result.Success)
                {
                    _logger.LogInformation($"OTP validated successfully in method ConfirmOTPDetails for email : {OTPDetails.Email} \n");

                    int deletedStatus = await _tokenRepository.DeleteTokenDetailsOnLogoutByMailAsync(OTPDetails.Email);

                    if (deletedStatus == 0 || deletedStatus != 0)
                    {

                        TokenDetails details = new TokenDetails();

                        var userDetails = await _userRepository.GetUserByEmailAsync(OTPDetails.Email);

                        _logger.LogInformation($"Creating auth claims to confirm OTP in method ConfirmOTPDetails for email : {OTPDetails.Email} \n");
                        var authClaims = new List<Claim>
                        {

                        new Claim(ClaimTypes.Name,userDetails.UserName ),
                        new Claim(ClaimTypes.Email, userDetails.Email),
                        new Claim(ClaimTypes.Role,userDetails.UserRole)
                        };

                        _logger.LogInformation($"Completing auth Claims  to confirm OTP in method ConfirmOTPDetails for email : {OTPDetails.Email} \n");

                        _logger.LogInformation($"Generating JWT token for user/admin in method ConfirmOTPDetails for email : {OTPDetails.Email} \n");

                        var accessToken = _jwtService.GetToken(authClaims, userDetails.UserName, "A");

                        var refreshToken = _jwtService.GetToken(authClaims, userDetails.UserName, "R");

                        if (accessToken == null || refreshToken == null)
                        {
                            _logger.LogError($"Failed to generate JWT token for user/admin in method ConfirmOTPDetails for email : {OTPDetails.Email} \n");
                            return StatusCode(500, new ApiResponse<string>(500, $"Oops! Something went wrong on our end... Server error"));
                        }
                        else
                        {
                            var accessTokenDetails = await _tokenRepository.GenerateJwtTokenDetailsAsync(userDetails, accessToken);

                            var refreshTokenDetails = await _tokenRepository.GenerateRefreshTokenDetailsAsync(userDetails, refreshToken);

                            if (accessTokenDetails == null || refreshTokenDetails == null)
                            {
                                _logger.LogError($"Failed to generate token for user/admin in method ConfirmOTPDetails for email : {OTPDetails.Email} \n");
                                return StatusCode(500, new ApiResponse<string>(500, $"Oops! Something went wrong on our end... Server error"));
                            }

                            if (accessTokenDetails != null)
                            {
                                details.AccessTokenDetails = accessTokenDetails;
                            }

                            if (refreshTokenDetails != null)
                            {
                                details.RefreshTokenDetails = refreshTokenDetails;
                            }

                            _logger.LogInformation($"JWT token for user/admin completed in method ConfirmOTPDetails for email : {OTPDetails.Email} \n");
                            return StatusCode(200, new ApiResponse<TokenDetails>(200, $"Welcome back, {details.AccessTokenDetails.UserName}! Your login was successful.", details));
                        }

                    }
                }
                else
                {
                    _logger.LogError($"OTP validated failed in method ConfirmOTPDetails for email : {OTPDetails.Email} \n");
                }
                return StatusCode(500, new ApiResponse<string>(500, $"Oops! Something went wrong on our end... Server error"));
            }
            catch (SqlException sqlex)
            {
                if (sqlex.Number == 50000)
                {
                    if (sqlex.Message.Contains("UserName cannot be null."))
                    {
                        _logger.LogError($"Attempting to confirm OTP failed in method ConfirmOTPDetails for email : {OTPDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                    else if (sqlex.Message.Contains("OTP cannot be null."))
                    {
                        _logger.LogError($"Attempting to confirm OTP failed in method ConfirmOTPDetails for email : {OTPDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "OTP cannot be null"));
                    }
                    else if (sqlex.Message.Contains("OTP code must be 4 digits."))
                    {
                        _logger.LogError($"Attempting to confirm OTP failed in method ConfirmOTPDetails for email : {OTPDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "OTP code must be 4 digits."));
                    }
                    else if (sqlex.Message.Contains("Email cannot be null."))
                    {
                        _logger.LogError($"Attempting to confirm OTP failed in method ConfirmOTPDetails for email : {OTPDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Email cannot be null."));
                    }
                    else if (sqlex.Message.Contains("Email not exists."))
                    {
                        _logger.LogError($"Attempting to confirm OTP failed in method ConfirmOTPDetails for email : {OTPDetails.Email} : {sqlex.Message} \n");
                        return StatusCode(404, new ApiResponse<string>(404, "Email not exists."));
                    }
                    else if (sqlex.Message.Contains("User Id cannot be null."))
                    {
                        _logger.LogError($"Attempting to confirm OTP failed in method ConfirmOTPDetails for email : {OTPDetails.Email} : {sqlex.Message} \n");

                        return StatusCode(400, new ApiResponse<string>(400, "Invalid data..try again"));
                    }
                }
                _logger.LogError($"Sql exception error during confirm OTP  in method ConfirmOTPDetails for email : {OTPDetails.Email}  : {sqlex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, "Oops! Something went wrong on our end... Server error"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred  during confirm OTP  in method ConfirmOTPDetails for email : {OTPDetails.Email} : {ex.Message} \n");
                return StatusCode(500, new ApiResponse<string>(500, $"Oops! Something went wrong on our end... Server error"));
            }

        }
    }
}
