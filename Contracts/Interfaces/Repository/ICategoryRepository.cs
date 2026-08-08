using Contracts.DTOs.Category;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface ICategoryRepository : IRepositoryBase<Category, CategoryDto, CategoryCreateDto, CategoryUpdateDto>
{
}
