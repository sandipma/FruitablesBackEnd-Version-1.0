using FruitStoreServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FruitStoreServices.Implementation
{
    public class JwtRepository : IJwtRepository
    {
        public Task<UserDetails> GenerateJwtTokenDetailsAsync(string userName)
        {
            try
            {
            
                _logger.LogInformation($"Generating JWT token for user: {userName}");

                var userDetails = await _tokenRepository.GetUserByNameAsync(userName);

                if (userDetails != null)
                {
                    validToken = await _tokenRepository.ValidateValidTokenAndReturnAsync(userDetails.Email);
                }


                if (validToken.ValidToken == null && validToken.Flag == "no access token")
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[]
                        {
                    new Claim(ClaimTypes.Name, userDetails.UserName),
                    new Claim(ClaimTypes.Role, userDetails.UserRole),
                    new Claim(ClaimTypes.Email, userDetails.Email)
                }),
                        Expires = DateTime.UtcNow.AddSeconds(10),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    userDetails.Token = tokenHandler.WriteToken(token);

                    InsertAccessTokenDetails accessTokenDetails = new()
                    {
                        Email = userDetails.Email,
                        AccessToken = userDetails.Token
                    };

                    await _tokenRepository.InsertAccessTokenDetailsAsync(accessTokenDetails);
                }

                else if (validToken.ValidToken != null && validToken.Flag == "A")
                {
                    userDetails.Token = validToken.ValidToken;
                }
                _logger.LogInformation($"JWT token generated successfully for user: {userName}");
                return userDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during JWT token generation for user: {userName}");
                // Handle the exception as needed, you can rethrow it or return a default value
                throw;
            }
        }
    }
}
