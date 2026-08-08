using Contracts.DTOs.Supplier;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface ISupplierRepository : IRepositoryBase<Supplier, SupplierDto, SupplierCreateDto, SupplierUpdateDto>
{
}
