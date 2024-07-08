using FruitStoreModels.Address;

namespace FruitStoreRepositories.Interfaces
{
    public interface IAddressDetailsReposiotry
    {
        public Task<int> InsertAddressDetailsAsync(AddAddressDetails addAddressDetails);

        public Task<int> UpdateAddressSelectionAsync(int addresssId, string isAddressSelected);

        public Task<List<AddressDetails>> GetAllAddressDetailsByUserIdAsync(int userId);

        public Task<int> DeleteAddressByIdAsync(int addressId);
    }
}
