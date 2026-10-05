using Contracts.DTOs.Department;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Databases;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_API.BaseControllers;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DepartmentController : BaseController<Department, DepartmentDto, DepartmentCreateDto, DepartmentUpdateDto>
{
    private readonly RepositoryContext _context;

    public DepartmentController(IRepositoryWrapper repositoryWrapper, RepositoryContext context)
    {
        _repository = repositoryWrapper.Departments;
        _context = context;
    }

    [HttpDelete("{id}")]
    public override async Task<IActionResult> Delete([FromRoute] string id)
    {
        if (!int.TryParse(id, out var departmentId))
            return BadRequest(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "معرف القسم غير صالح." });

        var existingDept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == departmentId);
        if (existingDept == null)
            return NotFound(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "القسم غير موجود." });

        var linkedExitRequestsCount = await _context.ProductExitRequests.CountAsync(r => r.DepartmentId == departmentId && !r.IsDeleted && (r.Status == RequestStatus.Pending || r.Status == RequestStatus.SupervisorApproved || r.Status == RequestStatus.ManagerApproved));
        var linkedEntryRequestsCount = await _context.ProductEntryRequests.CountAsync(r => r.DepartmentId == departmentId && !r.IsDeleted && (r.Status == RequestStatus.Pending || r.Status == RequestStatus.SupervisorApproved || r.Status == RequestStatus.ManagerApproved));
        var linkedBinsCount = await _context.WarehouseBins.CountAsync(b => b.DepartmentId == departmentId && !b.IsDeleted);

        if (linkedExitRequestsCount > 0 || linkedEntryRequestsCount > 0 || linkedBinsCount > 0)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"لا يمكن حذف هذا القسم لوجود ارتباطات نشطة به ({linkedExitRequestsCount + linkedEntryRequestsCount} طلبات جارية، {linkedBinsCount} أرفف مستودع). يرجى معالجة الارتباطات أولاً."
            });
        }

        return await base.Delete(id);
    }
}
