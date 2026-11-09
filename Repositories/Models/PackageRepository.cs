using Contracts.DTOs.Package;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Models;

public class PackageRepository
    : RepositoryBase<Package, PackageDto, PackageCreateDto, PackageUpdateDto>, IPackageRepository
{
    public PackageRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<List<PackageDto>> GetAllWithItemsAsync()
    {
        try
        {
            var packages = await RepositoryContext.Packages
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Include(p => p.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Category)
                .Include(p => p.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.TrackedProduct)
                .Include(p => p.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.UntrackedItem)
                .ToListAsync();

            return packages.Select(p => new PackageDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                BranchId = p.BranchId,
                Items = p.Items.Select(i => new PackageItemDto
                {
                    Id = i.Id,
                    PackageId = i.PackageId,
                    CategoryId = i.CategoryId,
                    CategoryName = i.Category != null ? i.Category.Name : null,
                    TrackedProductId = i.TrackedProductId,
                    TrackedProductName = i.TrackedProduct != null ? i.TrackedProduct.Name : null,
                    TrackedProductSKU = i.TrackedProduct != null ? i.TrackedProduct.SKU : null,
                    UntrackedItemId = i.UntrackedItemId,
                    UntrackedItemName = i.UntrackedItem != null ? i.UntrackedItem.Name : null,
                    Quantity = i.Quantity
                }).ToList()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.logErrorWithException(ex, $"{nameof(Package)} ===> GetAllWithItemsAsync");
            return new List<PackageDto>();
        }
    }

    public async Task<PackageDto?> GetByIdWithItemsAsync(int id)
    {
        try
        {
            var p = await RepositoryContext.Packages
                .AsNoTracking()
                .Where(p => p.Id == id && !p.IsDeleted)
                .Include(p => p.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Category)
                .Include(p => p.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.TrackedProduct)
                .Include(p => p.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.UntrackedItem)
                .FirstOrDefaultAsync();

            if (p == null) return null;

            return new PackageDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                BranchId = p.BranchId,
                Items = p.Items.Select(i => new PackageItemDto
                {
                    Id = i.Id,
                    PackageId = i.PackageId,
                    CategoryId = i.CategoryId,
                    CategoryName = i.Category != null ? i.Category.Name : null,
                    TrackedProductId = i.TrackedProductId,
                    TrackedProductName = i.TrackedProduct != null ? i.TrackedProduct.Name : null,
                    TrackedProductSKU = i.TrackedProduct != null ? i.TrackedProduct.SKU : null,
                    UntrackedItemId = i.UntrackedItemId,
                    UntrackedItemName = i.UntrackedItem != null ? i.UntrackedItem.Name : null,
                    Quantity = i.Quantity
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.logErrorWithException(ex, $"{nameof(Package)} ===> GetByIdWithItemsAsync");
            return null;
        }
    }
}

