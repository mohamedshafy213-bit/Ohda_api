using ClosedXML.Excel;
using Contracts.DTOs.Compass;
using Contracts.DTOs.ProductItem;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Databases;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_API.BaseControllers;
using System.Text.RegularExpressions;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CompassController : BaseController<Compass, CompassDto, CompassCreateDto, CompassUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly RepositoryContext _context;

    public CompassController(IRepositoryWrapper repositoryWrapper, RepositoryContext context)
    {
        _repositoryWrapper = repositoryWrapper;
        _context = context;
        _repository = repositoryWrapper.Compasses;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] int? departmentId,
        [FromQuery] CompassType? type,
        [FromQuery] int? stateId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null)
    {
        var records = await _repositoryWrapper.Compasses.SearchCompassRecordsAsync(
            query, departmentId, type, stateId, startDate, endDate, pageNumber, pageSize);

        var dtos = records.Select(c => new CompassDto
        {
            Id = c.Id,
            SerialNumber = c.SerialNumber,
            ProductName = c.ProductName,
            RecipientName = c.RecipientName,
            DelivererName = c.Type == CompassType.Exit 
                ? (c.ProductExitRequest?.RequestedByUser?.PersonName ?? c.ProductExitRequest?.RequestedByUser?.Username ?? c.ProductExitRequest?.InsertUserCode)
                : (c.ProductEntryRequest?.ReceivedByUser?.PersonName ?? c.ProductEntryRequest?.ReceivedByUser?.Username ?? c.ProductEntryRequest?.FromSource),
            SupervisorName = c.Type == CompassType.Exit
                ? (c.ProductExitRequest?.Supervisor?.PersonName ?? c.ProductExitRequest?.Supervisor?.Username)
                : (c.ProductEntryRequest?.Supervisor?.PersonName ?? c.ProductEntryRequest?.Supervisor?.Username),
            ManagerName = c.Type == CompassType.Exit
                ? (c.ProductExitRequest?.Manager?.PersonName ?? c.ProductExitRequest?.Manager?.Username)
                : (c.ProductEntryRequest?.Manager?.PersonName ?? c.ProductEntryRequest?.Manager?.Username),
            ApprovalTrail = c.Type == CompassType.Exit
                ? c.ProductExitRequest?.ApprovalTrail
                : c.ProductEntryRequest?.ApprovalTrail,
            RequesterConfirmedDate = c.Type == CompassType.Exit
                ? c.ProductExitRequest?.RequesterConfirmedDate
                : c.ProductEntryRequest?.RequesterConfirmedDate,
            Purpose = c.Type == CompassType.Exit
                ? c.ProductExitRequest?.Purpose
                : (c.ProductEntryRequest?.FromSource != null ? $"توريد من: {c.ProductEntryRequest.FromSource}" : null),
            Place = c.Place,
            ExitDate = c.ExitDate,
            Type = c.Type,
            DepartmentId = c.DepartmentId,
            DepartmentName = c.Department?.Name,
            ProductStateId = c.ProductStateId,
            ProductStateName = c.ProductState?.Name,
            ProductExitRequestId = c.ProductExitRequestId,
            ProductEntryRequestId = c.ProductEntryRequestId,
            DocumentNumber = c.Type == CompassType.Exit
                ? $"DOC-OUT-{(c.ProductExitRequestId ?? c.Id)}"
                : (!string.IsNullOrWhiteSpace(c.ProductEntryRequest?.InvoiceNumber) ? c.ProductEntryRequest.InvoiceNumber : $"DOC-IN-{(c.ProductEntryRequestId ?? c.Id)}"),
            OriginalExitDocumentNumber = c.ProductExitRequestId.HasValue
                ? $"DOC-OUT-{c.ProductExitRequestId.Value}"
                : (c.Notes != null && c.Notes.Contains("RET-REQ-") ? "DOC-OUT-" + c.Notes.Split("RET-REQ-")[1].Split("]")[0].Split(" ")[0] : null),
            Notes = c.Notes
        }).ToList();

        return Ok(new SingleObjectResponseModel<List<CompassDto>>
        {
            IsDone = true,
            ReturnMessage = "Search completed successfully.",
            SingleObject = dtos
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportToExcel(
        [FromQuery] string? query,
        [FromQuery] int? departmentId,
        [FromQuery] CompassType? type,
        [FromQuery] int? stateId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var records = await _repositoryWrapper.Compasses.SearchCompassRecordsAsync(
            query, departmentId, type, stateId, startDate, endDate, 1, 5000);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("بوصلة العهد والأجهزة");
        worksheet.RightToLeft = true;

        // Headers
        worksheet.Cell(1, 1).Value = "رقم المستند";
        worksheet.Cell(1, 2).Value = "الرقم التسلسلي (S/N)";
        worksheet.Cell(1, 3).Value = "اسم الصنف / الجهاز";
        worksheet.Cell(1, 4).Value = "نوع الحركة";
        worksheet.Cell(1, 5).Value = "المستلم";
        worksheet.Cell(1, 6).Value = "المسلم / مقدم الطلب";
        worksheet.Cell(1, 7).Value = "الجهة / مكان الصرف";
        worksheet.Cell(1, 8).Value = "القسم";
        worksheet.Cell(1, 9).Value = "الحالة الفنية";
        worksheet.Cell(1, 10).Value = "التاريخ والوقت";
        worksheet.Cell(1, 11).Value = "المعتمد الأول (المشرف)";
        worksheet.Cell(1, 12).Value = "المعتمد الثاني (المدير)";
        worksheet.Cell(1, 13).Value = "مستند الصرف الأصلي المرتبط";
        worksheet.Cell(1, 14).Value = "الملاحظات والبيان";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
        headerRow.Style.Font.SetFontColor(XLColor.White);
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        int rowIdx = 2;
        foreach (var c in records)
        {
            string docNum = c.Type == CompassType.Exit
                ? $"DOC-OUT-{(c.ProductExitRequestId ?? c.Id)}"
                : (!string.IsNullOrWhiteSpace(c.ProductEntryRequest?.InvoiceNumber) ? c.ProductEntryRequest.InvoiceNumber : $"DOC-IN-{(c.ProductEntryRequestId ?? c.Id)}");

            string origDocNum = c.ProductExitRequestId.HasValue
                ? $"DOC-OUT-{c.ProductExitRequestId.Value}"
                : (c.Notes != null && c.Notes.Contains("RET-REQ-") ? "DOC-OUT-" + c.Notes.Split("RET-REQ-")[1].Split("]")[0].Split(" ")[0] : "-");

            string deliverer = c.Type == CompassType.Exit 
                ? (c.ProductExitRequest?.RequestedByUser?.PersonName ?? c.ProductExitRequest?.RequestedByUser?.Username ?? c.ProductExitRequest?.InsertUserCode ?? "-")
                : (c.ProductEntryRequest?.ReceivedByUser?.PersonName ?? c.ProductEntryRequest?.ReceivedByUser?.Username ?? c.ProductEntryRequest?.FromSource ?? "-");

            string supervisor = c.Type == CompassType.Exit
                ? (c.ProductExitRequest?.Supervisor?.PersonName ?? c.ProductExitRequest?.Supervisor?.Username ?? "-")
                : (c.ProductEntryRequest?.Supervisor?.PersonName ?? c.ProductEntryRequest?.Supervisor?.Username ?? "-");

            string manager = c.Type == CompassType.Exit
                ? (c.ProductExitRequest?.Manager?.PersonName ?? c.ProductExitRequest?.Manager?.Username ?? "-")
                : (c.ProductEntryRequest?.Manager?.PersonName ?? c.ProductEntryRequest?.Manager?.Username ?? "-");

            worksheet.Cell(rowIdx, 1).Value = docNum;
            worksheet.Cell(rowIdx, 2).Value = c.SerialNumber;
            worksheet.Cell(rowIdx, 3).Value = c.ProductName;
            worksheet.Cell(rowIdx, 4).Value = c.Type == CompassType.Entry ? "دخول" : "خروج";
            worksheet.Cell(rowIdx, 5).Value = c.RecipientName;
            worksheet.Cell(rowIdx, 6).Value = deliverer;
            worksheet.Cell(rowIdx, 7).Value = c.Place;
            worksheet.Cell(rowIdx, 8).Value = c.Department?.Name ?? "-";
            worksheet.Cell(rowIdx, 9).Value = c.ProductState?.Name ?? "-";
            worksheet.Cell(rowIdx, 10).Value = c.ExitDate.ToString("yyyy-MM-dd HH:mm");
            worksheet.Cell(rowIdx, 11).Value = supervisor;
            worksheet.Cell(rowIdx, 12).Value = manager;
            worksheet.Cell(rowIdx, 13).Value = origDocNum;
            worksheet.Cell(rowIdx, 14).Value = c.Notes ?? "-";

            if (rowIdx % 2 == 1)
            {
                worksheet.Row(rowIdx).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
            }

            rowIdx++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        string filename = $"Ohda_Compass_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }

    [HttpGet("template")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadTemplate()
    {
        using var workbook = new XLWorkbook();
        
        // 1. Main Data Entry Sheet
        var dataSheet = workbook.Worksheets.Add("بيانات البوصلة والتسجيل");
        dataSheet.RightToLeft = true;

        dataSheet.Cell(1, 1).Value = "رقم الفرع (Branch ID) *";
        dataSheet.Cell(1, 2).Value = "رقم التصنيف (Category ID)";
        dataSheet.Cell(1, 3).Value = "اسم الصنف / الجهاز (Product Name) *";
        dataSheet.Cell(1, 4).Value = "الرقم التسلسلي (Serial Number) *";
        dataSheet.Cell(1, 5).Value = "رقم الحالة الفنية (State ID)";
        dataSheet.Cell(1, 6).Value = "المستلم / حامل العهدة (Recipient Name)";
        dataSheet.Cell(1, 7).Value = "مكان التواجد / القسم (Place / Dept)";
        dataSheet.Cell(1, 8).Value = "تاريخ القيد / الخروج (Date - YYYY-MM-DD)";
        dataSheet.Cell(1, 9).Value = "نوع الحركة (1=دخول/مخزن, 2=خروج/عهدة)";
        dataSheet.Cell(1, 10).Value = "ملاحظات وبيان (Notes)";

        var headerRow = dataSheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
        headerRow.Style.Font.SetFontColor(XLColor.White);
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Sample guide rows
        dataSheet.Cell(2, 1).Value = 1;
        dataSheet.Cell(2, 2).Value = 1;
        dataSheet.Cell(2, 3).Value = "طابعة HP LaserJet 2015";
        dataSheet.Cell(2, 4).Value = "CNB1234567";
        dataSheet.Cell(2, 5).Value = 1;
        dataSheet.Cell(2, 6).Value = "ملازم أول / أحمد علي";
        dataSheet.Cell(2, 7).Value = "مكتب النظم والمعلومات";
        dataSheet.Cell(2, 8).Value = DateTime.UtcNow.ToString("yyyy-MM-dd");
        dataSheet.Cell(2, 9).Value = 2;
        dataSheet.Cell(2, 10).Value = "عهدة تشغيلية للمكتب";

        dataSheet.Cell(3, 1).Value = 1;
        dataSheet.Cell(3, 2).Value = 2;
        dataSheet.Cell(3, 3).Value = "Switch 3Com 4200 24-Port";
        dataSheet.Cell(3, 4).Value = "SW-3COM-98765";
        dataSheet.Cell(3, 5).Value = 1;
        dataSheet.Cell(3, 6).Value = "أمين المخزن";
        dataSheet.Cell(3, 7).Value = "المخزن الرئيسي";
        dataSheet.Cell(3, 8).Value = DateTime.UtcNow.ToString("yyyy-MM-dd");
        dataSheet.Cell(3, 9).Value = 1;
        dataSheet.Cell(3, 10).Value = "رصيد وارد للمستودع";

        dataSheet.Columns().AdjustToContents();

        // 2. Branches Lookup Sheet
        var branches = await _context.Branches.AsNoTracking().Where(b => !b.IsDeleted).ToListAsync();
        var branchSheet = workbook.Worksheets.Add("دليل الفروع (Branches)");
        branchSheet.RightToLeft = true;
        branchSheet.Cell(1, 1).Value = "رقم الفرع (Branch ID)";
        branchSheet.Cell(1, 2).Value = "كود الفرع (Code)";
        branchSheet.Cell(1, 3).Value = "اسم الفرع (Branch Name)";
        StyleLookupHeader(branchSheet.Row(1), "#047857");
        int bIdx = 2;
        foreach (var b in branches)
        {
            branchSheet.Cell(bIdx, 1).Value = b.Id;
            branchSheet.Cell(bIdx, 2).Value = b.Code;
            branchSheet.Cell(bIdx, 3).Value = b.Name;
            bIdx++;
        }
        branchSheet.Columns().AdjustToContents();

        // 3. Categories Lookup Sheet
        var categories = await _context.Categories.IgnoreQueryFilters().AsNoTracking().Where(c => !c.IsDeleted).ToListAsync();
        var catSheet = workbook.Worksheets.Add("دليل التصنيفات (Categories)");
        catSheet.RightToLeft = true;
        catSheet.Cell(1, 1).Value = "رقم التصنيف (Category ID)";
        catSheet.Cell(1, 2).Value = "اسم التصنيف (Category Name)";
        catSheet.Cell(1, 3).Value = "الوصف (Description)";
        StyleLookupHeader(catSheet.Row(1), "#2563EB");
        int cIdx = 2;
        foreach (var c in categories)
        {
            catSheet.Cell(cIdx, 1).Value = c.Id;
            catSheet.Cell(cIdx, 2).Value = c.Name;
            catSheet.Cell(cIdx, 3).Value = c.Description ?? "-";
            cIdx++;
        }
        catSheet.Columns().AdjustToContents();

        // 4. Departments Lookup Sheet
        var departments = await _context.Departments.IgnoreQueryFilters().AsNoTracking().Where(d => !d.IsDeleted).ToListAsync();
        var deptSheet = workbook.Worksheets.Add("دليل الأقسام (Departments)");
        deptSheet.RightToLeft = true;
        deptSheet.Cell(1, 1).Value = "رقم القسم (Department ID)";
        deptSheet.Cell(1, 2).Value = "اسم القسم (Department Name)";
        StyleLookupHeader(deptSheet.Row(1), "#D97706");
        int dIdx = 2;
        foreach (var d in departments)
        {
            deptSheet.Cell(dIdx, 1).Value = d.Id;
            deptSheet.Cell(dIdx, 2).Value = d.Name;
            dIdx++;
        }
        deptSheet.Columns().AdjustToContents();

        // 5. Product States Lookup Sheet
        var states = await _context.ProductStates.IgnoreQueryFilters().AsNoTracking().Where(s => !s.IsDeleted).ToListAsync();
        var stateSheet = workbook.Worksheets.Add("دليل الحالات الفنية (States)");
        stateSheet.RightToLeft = true;
        stateSheet.Cell(1, 1).Value = "رقم الحالة (State ID)";
        stateSheet.Cell(1, 2).Value = "اسم الحالة (State Name)";
        stateSheet.Cell(1, 3).Value = "الكود (Code)";
        StyleLookupHeader(stateSheet.Row(1), "#4F46E5");
        int sIdx = 2;
        foreach (var s in states)
        {
            stateSheet.Cell(sIdx, 1).Value = s.Id;
            stateSheet.Cell(sIdx, 2).Value = s.Name;
            stateSheet.Cell(sIdx, 3).Value = s.Code ?? "-";
            sIdx++;
        }
        stateSheet.Columns().AdjustToContents();

        // 6. Existing Products Lookup Sheet (if any)
        var products = await _context.Products.Include(p => p.Category).IgnoreQueryFilters().AsNoTracking().Where(p => !p.IsDeleted).Take(500).ToListAsync();
        if (products.Any())
        {
            var prodSheet = workbook.Worksheets.Add("دليل المنتجات (Products)");
            prodSheet.RightToLeft = true;
            prodSheet.Cell(1, 1).Value = "رقم المنتج (Product ID)";
            prodSheet.Cell(1, 2).Value = "اسم المنتج (Product Name)";
            prodSheet.Cell(1, 3).Value = "التصنيف (Category)";
            prodSheet.Cell(1, 4).Value = "كود المنتج (SKU)";
            StyleLookupHeader(prodSheet.Row(1), "#475569");
            int pIdx = 2;
            foreach (var p in products)
            {
                prodSheet.Cell(pIdx, 1).Value = p.Id;
                prodSheet.Cell(pIdx, 2).Value = p.Name;
                prodSheet.Cell(pIdx, 3).Value = p.Category?.Name ?? "-";
                prodSheet.Cell(pIdx, 4).Value = p.SKU;
                pIdx++;
            }
            prodSheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Compass_Master_Template.xlsx");
    }

    private static void StyleLookupHeader(IXLRow row, string hexColor)
    {
        row.Style.Font.Bold = true;
        row.Style.Fill.BackgroundColor = XLColor.FromHtml(hexColor);
        row.Style.Font.SetFontColor(XLColor.White);
        row.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    [HttpPost("import")]
    [Authorize]
    public async Task<IActionResult> ImportCompassExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "الرجاء اختيار ملف إكسيل صالح للرفع."
            });
        }

        try
        {
            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().ToList();

            if (rows.Count <= 1)
            {
                return BadRequest(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = "ملف الإكسيل فارغ أو لا يحتوي على صفوف بيانات صالحة."
                });
            }

            var headerCells = rows[0].Cells().Select(c => c.GetValue<string>().Trim()).ToList();
            bool isNewTemplate = headerCells.Count >= 7 || headerCells.Any(h => h.Contains("Branch") || h.Contains("الفرع") || h.Contains("Category") || h.Contains("التصنيف"));

            var allBranches = await _context.Branches.AsNoTracking().Where(b => !b.IsDeleted).ToListAsync();
            var allCategories = await _context.Categories.IgnoreQueryFilters().AsNoTracking().Where(c => !c.IsDeleted).ToListAsync();
            var allDepartments = await _context.Departments.IgnoreQueryFilters().AsNoTracking().Where(d => !d.IsDeleted).ToListAsync();
            var allStates = await _context.ProductStates.IgnoreQueryFilters().AsNoTracking().Where(s => !s.IsDeleted).ToListAsync();

            int defaultBranchId = _context.CurrentBranchId ?? allBranches.FirstOrDefault()?.Id ?? 1;
            int defaultCategoryId = allCategories.FirstOrDefault()?.Id ?? 1;
            int defaultStateId = allStates.FirstOrDefault()?.Id ?? 1;

            var currentUsername = User.Identity?.Name ?? "CompassImporter";
            var userEntity = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == currentUsername || u.MilitaryNumber.ToString() == currentUsername);
            var userCode = userEntity?.PersonName ?? currentUsername;

            int createdProductsCount = 0;
            int createdProductItemsCount = 0;
            int updatedProductItemsCount = 0;
            int createdInventoriesCount = 0;
            int importedCompassCount = 0;

            var dataRows = rows.Skip(1);

            foreach (var row in dataRows)
            {
                int branchId = defaultBranchId;
                int categoryId = defaultCategoryId;
                string productName = "";
                string serialNumber = "";
                int stateId = defaultStateId;
                string recipientName = "";
                string place = "";
                DateTime exitDate = DateTime.UtcNow;
                CompassType type = CompassType.Exit;
                string notes = "";

                if (isNewTemplate)
                {
                    string branchVal = row.Cell(1).GetValue<string>().Trim();
                    if (int.TryParse(branchVal, out int bId) && allBranches.Any(b => b.Id == bId))
                    {
                        branchId = bId;
                    }
                    else if (!string.IsNullOrWhiteSpace(branchVal))
                    {
                        var foundBranch = allBranches.FirstOrDefault(b => b.Code.Equals(branchVal, StringComparison.OrdinalIgnoreCase) || b.Name.Equals(branchVal, StringComparison.OrdinalIgnoreCase));
                        if (foundBranch != null) branchId = foundBranch.Id;
                    }

                    string catVal = row.Cell(2).GetValue<string>().Trim();
                    if (int.TryParse(catVal, out int cId) && allCategories.Any(c => c.Id == cId))
                    {
                        categoryId = cId;
                    }
                    else if (!string.IsNullOrWhiteSpace(catVal))
                    {
                        var foundCat = allCategories.FirstOrDefault(c => c.Name.Equals(catVal, StringComparison.OrdinalIgnoreCase));
                        if (foundCat != null)
                        {
                            categoryId = foundCat.Id;
                        }
                        else
                        {
                            var newCat = new Category { Name = catVal, BranchId = branchId, InsertDate = DateTime.UtcNow, InsertUserCode = userCode };
                            _context.Categories.Add(newCat);
                            await _context.SaveChangesAsync();
                            allCategories.Add(newCat);
                            categoryId = newCat.Id;
                        }
                    }

                    productName = row.Cell(3).GetValue<string>().Trim();
                    serialNumber = row.Cell(4).GetValue<string>().Trim();

                    string stateVal = row.Cell(5).GetValue<string>().Trim();
                    if (int.TryParse(stateVal, out int sId) && allStates.Any(s => s.Id == sId))
                    {
                        stateId = sId;
                    }
                    else if (!string.IsNullOrWhiteSpace(stateVal))
                    {
                        var foundState = allStates.FirstOrDefault(s => s.Name.Contains(stateVal, StringComparison.OrdinalIgnoreCase));
                        if (foundState != null) stateId = foundState.Id;
                    }

                    recipientName = row.Cell(6).GetValue<string>().Trim();
                    place = row.Cell(7).GetValue<string>().Trim();

                    string dateVal = row.Cell(8).GetValue<string>().Trim();
                    if (DateTime.TryParse(dateVal, out var parsedDate))
                    {
                        exitDate = parsedDate;
                    }

                    string typeVal = row.Cell(9).GetValue<string>().Trim();
                    if (typeVal == "1" || typeVal.Contains("دخول") || typeVal.Equals("Entry", StringComparison.OrdinalIgnoreCase) || typeVal.Contains("مخزن"))
                    {
                        type = CompassType.Entry;
                    }
                    else
                    {
                        type = CompassType.Exit;
                    }

                    notes = row.Cell(10).GetValue<string>().Trim();
                }
                else
                {
                    serialNumber = row.Cell(1).GetValue<string>().Trim();
                    productName = row.Cell(2).GetValue<string>().Trim();
                    recipientName = row.Cell(3).GetValue<string>().Trim();
                    place = row.Cell(4).GetValue<string>().Trim();
                    string dateVal = row.Cell(5).GetValue<string>().Trim();
                    if (DateTime.TryParse(dateVal, out var parsedDate))
                    {
                        exitDate = parsedDate;
                    }
                    notes = row.Cell(6).GetValue<string>().Trim();
                }

                if (string.IsNullOrWhiteSpace(productName) && string.IsNullOrWhiteSpace(serialNumber))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(productName))
                {
                    productName = "صنف (" + serialNumber + ")";
                }

                if (string.IsNullOrWhiteSpace(serialNumber))
                {
                    serialNumber = $"SN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                }

                int? departmentId = null;
                if (!string.IsNullOrWhiteSpace(place))
                {
                    var foundDept = allDepartments.FirstOrDefault(d => d.Name.Equals(place, StringComparison.OrdinalIgnoreCase) || place.Contains(d.Name));
                    if (foundDept != null)
                    {
                        departmentId = foundDept.Id;
                    }
                }

                // Step 1: Ensure Category exists
                if (categoryId == 0 || !allCategories.Any(c => c.Id == categoryId))
                {
                    var firstCat = allCategories.FirstOrDefault();
                    if (firstCat == null)
                    {
                        firstCat = new Category { Name = "أجهزة ومعدات عامة", BranchId = branchId, InsertDate = DateTime.UtcNow, InsertUserCode = userCode };
                        _context.Categories.Add(firstCat);
                        await _context.SaveChangesAsync();
                        allCategories.Add(firstCat);
                    }
                    categoryId = firstCat.Id;
                }

                // Step 2: Auto-Create Product in Products Table if not existing
                var existingProduct = await _context.Products.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == productName.ToLower() && p.BranchId == branchId && !p.IsDeleted);

                int productId;
                if (existingProduct == null)
                {
                    var newProduct = new Product
                    {
                        Name = productName,
                        CategoryId = categoryId,
                        BranchId = branchId,
                        SKU = $"PRD-{DateTime.UtcNow.Ticks.ToString().Substring(11)}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                        Barcode = $"BAR-{DateTime.UtcNow.Ticks.ToString().Substring(11)}",
                        InventoryType = InventoryType.Purchased,
                        UnitPrice = 0,
                        PurchasePrice = 0,
                        AssetValue = 0,
                        InsertDate = DateTime.UtcNow,
                        InsertUserCode = userCode
                    };
                    _context.Products.Add(newProduct);
                    await _context.SaveChangesAsync();
                    createdProductsCount++;
                    productId = newProduct.Id;
                }
                else
                {
                    productId = existingProduct.Id;
                }

                // Step 3: Ensure Inventory record exists in Inventories Table
                var existingInventory = await _context.Inventories.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(i => i.ProductId == productId && i.BranchId == branchId && !i.IsDeleted);

                if (existingInventory == null)
                {
                    existingInventory = new Inventory
                    {
                        ProductId = productId,
                        BranchId = branchId,
                        Quantity = (type == CompassType.Entry ? 1 : 0),
                        MinStock = 1,
                        MaxStock = 100,
                        InsertDate = DateTime.UtcNow,
                        InsertUserCode = userCode
                    };
                    _context.Inventories.Add(existingInventory);
                    await _context.SaveChangesAsync();
                    createdInventoriesCount++;
                }
                else if (type == CompassType.Entry)
                {
                    existingInventory.Quantity += 1;
                    existingInventory.LastUpdate = DateTime.UtcNow;
                    existingInventory.UpdateUserCode = userCode;
                }

                // Step 4: Create or Update serialized ProductItem
                var existingProductItem = await _context.ProductItems.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(pi => pi.SerialNumber == serialNumber && pi.BranchId == branchId && !pi.IsDeleted);

                if (existingProductItem == null)
                {
                    var newProductItem = new ProductItem
                    {
                        ProductId = productId,
                        BranchId = branchId,
                        SerialNumber = serialNumber,
                        QRCode = serialNumber,
                        Status = (type == CompassType.Entry ? ProductItemStatus.InStock : ProductItemStatus.Exited),
                        RecipientName = (type == CompassType.Exit ? recipientName : null),
                        Place = (type == CompassType.Exit ? place : null),
                        ExitDate = (type == CompassType.Exit ? exitDate : null),
                        Notes = notes,
                        InsertDate = DateTime.UtcNow,
                        InsertUserCode = userCode
                    };
                    _context.ProductItems.Add(newProductItem);
                    createdProductItemsCount++;
                }
                else
                {
                    existingProductItem.ProductId = productId;
                    existingProductItem.Status = (type == CompassType.Entry ? ProductItemStatus.InStock : ProductItemStatus.Exited);
                    if (type == CompassType.Exit)
                    {
                        existingProductItem.RecipientName = recipientName;
                        existingProductItem.Place = place;
                        existingProductItem.ExitDate = exitDate;
                    }
                    if (!string.IsNullOrWhiteSpace(notes))
                    {
                        existingProductItem.Notes = string.IsNullOrWhiteSpace(existingProductItem.Notes)
                            ? notes
                            : $"{existingProductItem.Notes} | {notes}";
                    }
                    existingProductItem.LastUpdate = DateTime.UtcNow;
                    existingProductItem.UpdateUserCode = userCode;
                    updatedProductItemsCount++;
                }

                // Step 5: Create or Update Compass Log
                var existingCompass = await _context.Compasses.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.SerialNumber == serialNumber && c.BranchId == branchId && !c.IsDeleted);

                if (existingCompass == null)
                {
                    var newCompass = new Compass
                    {
                        BranchId = branchId,
                        SerialNumber = serialNumber,
                        ProductName = productName,
                        RecipientName = recipientName,
                        Place = place,
                        ExitDate = exitDate,
                        Type = type,
                        DepartmentId = departmentId,
                        ProductStateId = stateId,
                        Notes = notes,
                        InsertDate = DateTime.UtcNow,
                        InsertUserCode = userCode
                    };
                    _context.Compasses.Add(newCompass);
                    importedCompassCount++;
                }
                else
                {
                    existingCompass.ProductName = productName;
                    existingCompass.RecipientName = recipientName;
                    existingCompass.Place = place;
                    existingCompass.ExitDate = exitDate;
                    existingCompass.Type = type;
                    if (departmentId.HasValue) existingCompass.DepartmentId = departmentId.Value;
                    if (stateId > 0) existingCompass.ProductStateId = stateId;
                    existingCompass.Notes = notes;
                    existingCompass.LastUpdate = DateTime.UtcNow;
                    existingCompass.UpdateUserCode = userCode;
                }
            }

            await _context.SaveChangesAsync();

            string summaryMsg = $"تمت العملية بنجاح! تم تسجيل {importedCompassCount} حركة بوصلة، وإنشاء {createdProductsCount} صنف جديد بجدول المنتجات، وتسجيل {createdProductItemsCount} سيريال جديد، وتحديث {updatedProductItemsCount} جهاز قائم.";

            return Ok(new SingleObjectResponseModel
            {
                IsDone = true,
                ReturnMessage = summaryMsg
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"حدث خطأ أثناء استيراد ملف الإكسيل: {ex.Message}"
            });
        }
    }
}

