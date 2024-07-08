using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FruitStoreServices.Interfaces
{
    public interface IJwtRepository
    {
        Task<string> GenerateJwtTokenDetailsAsync(string userName);
    }
}
