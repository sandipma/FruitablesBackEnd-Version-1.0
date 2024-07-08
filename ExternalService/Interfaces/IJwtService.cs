using System.Security.Claims;

namespace ExternalService.Interfaces
{
    public interface IJwtService
    {
        string GetToken(List<Claim> authClaims, string userName, string flag);
    }
}
