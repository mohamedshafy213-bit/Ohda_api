using Contracts.DTOs.Package;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts.Interfaces.Repository;

public interface IPackageRepository : IRepositoryBase<Package, PackageDto, PackageCreateDto, PackageUpdateDto>
{
    Task<List<PackageDto>> GetAllWithItemsAsync();
    Task<PackageDto?> GetByIdWithItemsAsync(int id);
}

