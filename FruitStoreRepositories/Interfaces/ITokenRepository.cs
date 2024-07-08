using FruitStoreModels.ApplicationUser;
using FruitStoreModels.Auth;

namespace FruitStoreRepositories.InterFaces
{
    public interface ITokenRepository
    {
        Task<AccessTokenDetails> GenerateJwtTokenDetailsAsync(UserDetails userDetails, string token);

        Task<RefreshTokenDetails> GenerateRefreshTokenDetailsAsync(UserDetails userdetails, string token);

        Task<AccessTokenDetails> GetAccessTokenByEmailDetailsAsync(string email);

        Task<RefreshTokenDetails> GetRefreshTokenByEmailDetailsAsync(string email);

        Task<string> InsertAccessTokenDetailsAsync(InsertAccessTokenDetails insertAccessTokenDetails);

        Task<string> InsertRefreshTokenDetailsAsync(InsertRefreshTokenDetails insertRefreshTokenDetails);

        Task<int> DeleteTokenDetailsOnLogoutByMailAsync(string email);

        Task SetTokenSchedularAsync();
    }
}
