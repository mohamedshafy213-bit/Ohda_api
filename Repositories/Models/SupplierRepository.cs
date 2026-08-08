using Contracts.DTOs.Supplier;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Repositories;

namespace Repositories.Models;

public class SupplierRepository
    : RepositoryBase<Supplier, SupplierDto, SupplierCreateDto, SupplierUpdateDto>, ISupplierRepository
{
    public SupplierRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }
}
