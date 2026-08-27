using Contracts.DTOs.ProductState;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IProductStateRepository : IRepositoryBase<ProductState, ProductStateDto, ProductStateCreateDto, ProductStateUpdateDto>
{
}
