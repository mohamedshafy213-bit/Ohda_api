using Contracts.DTOs.ProductEntryRequest;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IProductEntryRequestRepository : IRepositoryBase<ProductEntryRequest, ProductEntryRequestDto, ProductEntryCreateDto, ProductEntryUpdateDto>
{
    Task<ProductEntryRequest?> GetByIdAsync(int id);
}
