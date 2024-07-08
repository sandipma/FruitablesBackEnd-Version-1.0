using ExternalService.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExternalService.Implementation
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;
        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Method for token generation i.e Access token and Refresh Token //
        public string GetToken(List<Claim> authClaims, string userName, string flag)
        {
            try
            {
                _logger.LogInformation($"Starting JWT token generation process in method GetToken for user: {userName} \n");

                string tokenString = string.Empty;

                JwtSecurityToken? token = null;

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

                if (flag == "A")
                {
                    _logger.LogInformation("Starting token with expiration (15 min) for user \n");

                    token = new JwtSecurityToken(
                        issuer: _configuration["JWT:Issuer"],
                        audience: _configuration["JWT:Audience"],
                        expires: DateTime.Now.AddMinutes(120),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                    tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                    _logger.LogInformation("Completed token generation expiration (15 min) for user \n");
                }
                else
                {
                    _logger.LogInformation("Generating refresh token with expiration (24 hours) user \n");

                    token = new JwtSecurityToken(
                        issuer: _configuration["JWT:Issuer"],
                        audience: _configuration["JWT:Audience"],
                        expires: DateTime.Now.AddMinutes(125),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                    tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                    _logger.LogInformation("Completed token generation with expiration (24 hours) for user \n");
                }

                _logger.LogInformation($"JWT token generated successfully in method GetToken for user : {userName} \n");

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating JWT token in method GetToken for user : {userName} : {ex.Message} \n");
                throw;
            }
        }
    }
}
