using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.DTOs.Package;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Databases;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PackageController : BaseController<Package, PackageDto, PackageCreateDto, PackageUpdateDto>
{
    private readonly RepositoryContext _context;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public PackageController(IRepositoryWrapper repositoryWrapper, RepositoryContext context)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.Packages;
        _context = context;
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        var list = await _repositoryWrapper.Packages.GetAllWithItemsAsync();
        return Ok(new JsonResponseModel
        {
            IsDone = true,
            ReturnMessage = "تم جلب الحزم بنجاح",
            JsonResult = list
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pkg = await _repositoryWrapper.Packages.GetByIdWithItemsAsync(id);
        if (pkg == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "الحزمة غير موجودة"
            });
        }

        return Ok(new SingleObjectResponseModel<PackageDto>
        {
            IsDone = true,
            ReturnMessage = "تم جلب بيانات الحزمة بنجاح",
            SingleObject = pkg
        });
    }

    [HttpPost]
    public override async Task<IActionResult> Create([FromBody] PackageCreateDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "اسم الحزمة مطلوب"
            });
        }

        var pkg = new Package
        {
            Name = createDto.Name.Trim(),
            Description = createDto.Description,
            IsActive = createDto.IsActive,
            BranchId = createDto.BranchId ?? 1,
            Items = createDto.Items.Select(i => new PackageItem
            {
                CategoryId = i.CategoryId,
                TrackedProductId = i.TrackedProductId,
                UntrackedItemId = i.UntrackedItemId,
                Quantity = i.UntrackedItemId.HasValue ? 1 : Math.Max(1, i.Quantity),
                BranchId = createDto.BranchId ?? 1
            }).ToList()
        };

        await _context.Packages.AddAsync(pkg);
        await _context.SaveChangesAsync();

        var result = await _repositoryWrapper.Packages.GetByIdWithItemsAsync(pkg.Id);
        return Ok(new SingleObjectResponseModel<PackageDto>
        {
            IsDone = true,
            ReturnMessage = "تم إنشاء الحزمة بنجاح",
            SingleObject = result!
        });
    }

    [HttpPut("{id}")]
    public override async Task<IActionResult> Update([FromRoute] string id, [FromBody] PackageUpdateDto updateDto)
    {
        if (!int.TryParse(id, out var intId))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "المعرف غير صحيح"
            });
        }

        var pkg = await _context.Packages
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == intId && !p.IsDeleted);

        if (pkg == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "الحزمة غير موجودة"
            });
        }

        pkg.Name = updateDto.Name.Trim();
        pkg.Description = updateDto.Description;
        pkg.IsActive = updateDto.IsActive;

        // Remove old items
        _context.PackageItems.RemoveRange(pkg.Items);

        // Add updated items
        pkg.Items = updateDto.Items.Select(i => new PackageItem
        {
            PackageId = pkg.Id,
            CategoryId = i.CategoryId,
            TrackedProductId = i.TrackedProductId,
            UntrackedItemId = i.UntrackedItemId,
            Quantity = i.UntrackedItemId.HasValue ? 1 : Math.Max(1, i.Quantity),
            BranchId = pkg.BranchId
        }).ToList();

        await _context.SaveChangesAsync();

        var result = await _repositoryWrapper.Packages.GetByIdWithItemsAsync(pkg.Id);
        return Ok(new SingleObjectResponseModel<PackageDto>
        {
            IsDone = true,
            ReturnMessage = "تم تحديث الحزمة بنجاح",
            SingleObject = result!
        });
    }

    [HttpDelete("{id}")]
    public override async Task<IActionResult> Delete([FromRoute] string id)
    {
        if (!int.TryParse(id, out var intId))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "المعرف غير صحيح"
            });
        }

        var pkg = await _context.Packages.FirstOrDefaultAsync(p => p.Id == intId && !p.IsDeleted);
        if (pkg == null)
        {
            return NotFound(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "الحزمة غير موجودة"
            });
        }

        pkg.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new SingleObjectResponseModel<bool>
        {
            IsDone = true,
            ReturnMessage = "تم حذف الحزمة بنجاح",
            SingleObject = true
        });
    }
}
