using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Contracts.DTOs.Department;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Databases;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_API.BaseControllers;
using Service_API.Helpers;

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

    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        var effectivePageSize = pageSize ?? 1000;
        var response = await _repository.FindAll(pageNumber, effectivePageSize);
        return HandleResponse(response);
    }

    [HttpGet("template")]
    [AllowAnonymous]
    public IActionResult DownloadTemplate()
    {
        using var workbook = DepartmentExcelHelper.CreateDepartmentsWorkbook();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "College_Departments_Demo_200.xlsx"
        );
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var departments = await _context.Departments
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.Id)
            .Select(d => new { d.Name, Description = d.Description ?? "" })
            .ToListAsync();

        using var workbook = DepartmentExcelHelper.CreateDepartmentsWorkbook(
            departments.Select(d => (d.Name, d.Description))
        );
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Departments_Export_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        );
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile? file)
    {
        file ??= Request.Form.Files.FirstOrDefault();
        if (file == null || file.Length == 0)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "يرجى اختيار ملف Excel صالح بصيغة (.xlsx)."
            });
        }

        try
        {
            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                return BadRequest(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = "الملف المرفق فارغ ولا يحتوي على أي أوراق عمل."
                });
            }

            var rows = worksheet.RowsUsed().Skip(1); // Skip header row
            var existingDepartments = await _context.Departments
                .Where(d => !d.IsDeleted)
                .ToListAsync();

            var existingByName = existingDepartments
                .GroupBy(d => d.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            int totalRows = 0;
            int addedCount = 0;
            int updatedCount = 0;
            int skippedCount = 0;
            var toAdd = new List<Department>();

            foreach (var row in rows)
            {
                string col1 = row.Cell(1).Value.ToString().Trim();
                string col2 = row.Cell(2).Value.ToString().Trim();
                string col3 = row.Cell(3).Value.ToString().Trim();

                string name;
                string description;

                // Support both layouts:
                // Layout A: Col 1 = No/م, Col 2 = Name, Col 3 = Description
                // Layout B: Col 1 = Name, Col 2 = Description
                if (int.TryParse(col1, out _) && !string.IsNullOrWhiteSpace(col2))
                {
                    name = col2;
                    description = col3;
                }
                else if (!string.IsNullOrWhiteSpace(col1))
                {
                    name = col1;
                    description = col2;
                }
                else if (!string.IsNullOrWhiteSpace(col2))
                {
                    name = col2;
                    description = col3;
                }
                else
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                totalRows++;

                if (existingByName.TryGetValue(name, out var existing))
                {
                    if (!string.IsNullOrWhiteSpace(description) && existing.Description != description)
                    {
                        existing.Description = description;
                        existing.LastUpdate = DateTime.UtcNow;
                        updatedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
                else
                {
                    var newDept = new Department
                    {
                        Name = name,
                        Description = string.IsNullOrWhiteSpace(description) ? null : description,
                        InsertDate = DateTime.UtcNow
                    };
                    toAdd.Add(newDept);
                    existingByName[name] = newDept;
                    addedCount++;
                }
            }

            if (toAdd.Count > 0)
            {
                await _context.Departments.AddRangeAsync(toAdd);
            }

            await _context.SaveChangesAsync();
            _repository.InvalidateCache();

            string returnMsg = addedCount > 0
                ? $"تم استيراد البيانات بنجاح: تم إضافة {addedCount} قسم جديد، وتحديث {updatedCount} قسم، وتخطي {skippedCount} قسم موجود مسبقاً (إجمالي {totalRows} صف)."
                : $"تم التحقق بنجاح: جميع الأقسام المذكورة في الملف ({skippedCount} قسم) مسجلة وموجودة بالفعل في النظام.";

            return Ok(new SingleObjectResponseModel<object>
            {
                IsDone = true,
                ReturnMessage = returnMsg,
                SingleObject = new
                {
                    totalProcessed = totalRows,
                    added = addedCount,
                    updated = updatedCount,
                    skipped = skippedCount
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"حدث خطأ أثناء معالجة ملف Excel: {ex.Message}"
            });
        }
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
