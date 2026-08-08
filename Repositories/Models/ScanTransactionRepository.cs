using Contracts.DTOs.ScanTransaction;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Repositories;

namespace Repositories.Models;

public class ScanTransactionRepository
    : RepositoryBase<ScanTransaction, ScanTransactionDto, ScanTransactionCreateDto, ScanTransactionUpdateDto>, IScanTransactionRepository
{
    public ScanTransactionRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }
}
