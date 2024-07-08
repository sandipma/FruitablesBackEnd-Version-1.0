using System.ComponentModel.DataAnnotations;

namespace CommonHelperUtility
{
    public static class UserValidator
    {
        public static bool IsValidEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }
        public static string ConvertImageTo64(byte[] imagedata)
        {
            string finalData = string.Empty;
            if (imagedata != null)
            {
                string base64String = Convert.ToBase64String(imagedata);
                finalData = base64String;
            }

            return finalData;
        }
    }
}
