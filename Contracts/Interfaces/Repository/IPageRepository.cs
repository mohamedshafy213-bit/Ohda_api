using Contracts.DTOs.Page;
using Contracts.interfaces.Repository;
using Entities.Models.Enums;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IPageRepository : IRepositoryBase<Page, PageDto, PageCreateDto, PageUpdateDto>
{
    Task<List<PageDto>> GetAllowedPagesForRoleAsync(UserRole role);
}
