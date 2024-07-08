using FruitStoreModels.ApplicationUser;
using FruitStoreModels.Auth;

namespace FruitStoreRepositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<int> InsertUserAsync(RegisterUser registerUser);

        public Task<int> InsertForgetPasswordAsync(ForgotPasswordDetails forgotPassword);

        public Task<UserDetails> GetUserByUserIdAsync(int userId);

        public Task<ForgotPasswordDetails> GetCodeDetailsByUserIdAsync(int userId);

        public Task<UserDetails> GetUserByEmailAsync(string email);

        public Task<UserDetails> GetUserByNameAsync(string UserName);

        public Task<int> UpdatePasswordByMailAsync(string email, string encrytedPassword);

        public Task<OperationResult<string>> ForgotPaswordEmailAsync(AddForgotPasswordDetails email);

        public Task<OperationResult<string>> ResetPasswordAsync(ResetPassword model);

        public Task<OperationResult<string>> SendOTPByEmailAsync(AddOTPDetails OTPDetails);

        public Task<int> InsertOTPAsync(InsertOTPDetails OTPDetails);

        public Task<OTPDetails> GetOTPDetailsByUserIdAsync(int userId);

        public Task<OperationResult<string>> ConfirmOTPAsync(ConfirmOTP confirmOTP);
    }
}
