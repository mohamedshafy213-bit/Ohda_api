using Contracts.DTOs.Department;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IDepartmentRepository : IRepositoryBase<Department, DepartmentDto, DepartmentCreateDto, DepartmentUpdateDto>
{
}
