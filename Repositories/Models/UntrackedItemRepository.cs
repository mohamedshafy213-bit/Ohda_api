using Contracts.DTOs.UntrackedItem;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Repositories;

namespace Repositories.Models;

public class UntrackedItemRepository
    : RepositoryBase<UntrackedItem, UntrackedItemDto, UntrackedItemCreateDto, UntrackedItemUpdateDto>, IUntrackedItemRepository
{
    public UntrackedItemRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }
}

