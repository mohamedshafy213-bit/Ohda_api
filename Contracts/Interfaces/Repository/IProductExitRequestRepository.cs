using Contracts.DTOs.ProductExitRequest;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IProductExitRequestRepository : IRepositoryBase<ProductExitRequest, ProductExitRequestDto, ProductExitCreateDto, ProductExitUpdateDto>
{
    Task<ProductExitRequest?> GetByIdAsync(int id);
}
