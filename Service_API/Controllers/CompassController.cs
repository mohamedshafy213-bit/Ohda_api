using ClosedXML.Excel;
using Contracts.DTOs.Compass;
using Contracts.DTOs.ProductItem;
using Entities.Models.Enums;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;
using System.Text.RegularExpressions;


namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CompassController : BaseController<Compass, CompassDto, CompassCreateDto, CompassUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public CompassController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
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
            Place = c.Place,
            ExitDate = c.ExitDate,
            Type = c.Type,
            DepartmentId = c.DepartmentId,
            DepartmentName = c.Department?.Name,
            ProductStateId = c.ProductStateId,
            ProductStateName = c.ProductState?.Name,
            ProductExitRequestId = c.ProductExitRequestId,
            ProductEntryRequestId = c.ProductEntryRequestId,
            Notes = c.Notes
        }).ToList();

        return Ok(new SingleObjectResponseModel<List<CompassDto>>
        {
            IsDone = true,
            ReturnMessage = "Search completed successfully.",
            SingleObject = dtos
        });
    }

    [HttpGet("template")]
    [AllowAnonymous] // Allow downloading template without strict auth if needed, or keep [Authorize]
    public IActionResult DownloadTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Compass_Template");

        // Headers (Arabic columns mapping to the document)
        worksheet.Cell(1, 1).Value = "الرقم التسلسلي (Serial Number)";
        worksheet.Cell(1, 2).Value = "اسم الجهاز / المنتج (Product Name)";
        worksheet.Cell(1, 3).Value = "اسم المستلم (Recipient Name)";
        worksheet.Cell(1, 4).Value = "الجهة / المكان (Place / Department)";
        worksheet.Cell(1, 5).Value = "تاريخ الخروج (Exit Date - YYYY-MM-DD)";
        worksheet.Cell(1, 6).Value = "ملاحظات (Notes)";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.Navy;
        headerRow.Style.Font.SetFontColor(XLColor.White);

        // Add some dummy row data to guide the user
        worksheet.Cell(2, 1).Value = "SN-12345678";
        worksheet.Cell(2, 2).Value = "Switch 3com superstack PS40 12 port";
        worksheet.Cell(2, 3).Value = "ملازم أول / أحمد محمد";
        worksheet.Cell(2, 4).Value = "مكتب النظم والمعلومات";
        worksheet.Cell(2, 5).Value = DateTime.Now.ToString("yyyy-MM-dd");
        worksheet.Cell(2, 6).Value = "تم التسليم للتشغيل";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Compass_Template.xlsx");
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin, Manager")]
    public async Task<IActionResult> ImportCompassExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Please select a valid Excel file."
            });
        }

        try
        {
            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // Skip headers

            int importedCount = 0;
            int updatedInventoryCount = 0;

            foreach (var row in rows)
            {
                string serialNumber = row.Cell(1).GetValue<string>().Trim();
                string productName = row.Cell(2).GetValue<string>().Trim();
                string recipientName = row.Cell(3).GetValue<string>().Trim();
                string place = row.Cell(4).GetValue<string>().Trim();
                
                string exitDateStr = row.Cell(5).GetValue<string>().Trim();
                DateTime exitDate = DateTime.UtcNow;
                if (DateTime.TryParse(exitDateStr, out var parsedDate))
                {
                    exitDate = parsedDate;
                }

                string notes = row.Cell(6).GetValue<string>().Trim();

                if (string.IsNullOrWhiteSpace(serialNumber) || string.IsNullOrWhiteSpace(productName) || 
                    string.IsNullOrWhiteSpace(recipientName) || string.IsNullOrWhiteSpace(place))
                {
                    continue; // Skip incomplete rows
                }

                // 1. Check if record already exists in Compass log
                var existingCompass = await _repositoryWrapper.Compasses.GetBySerialNumberAsync(serialNumber);
                if (existingCompass == null)
                {
                    await _repositoryWrapper.Compasses.Create(new CompassCreateDto
                    {
                        SerialNumber = serialNumber,
                        ProductName = productName,
                        RecipientName = recipientName,
                        Place = place,
                        ExitDate = exitDate,
                        Notes = notes
                    });
                    importedCount++;

                    // 2. Check if this serial number is in our inventory (ProductItem)
                    var productItem = await _repositoryWrapper.ProductItems.GetBySerialNumberAsync(serialNumber);
                    if (productItem != null && productItem.Status == Entities.Models.Enums.ProductItemStatus.InStock)
                    {
                        // Mark as exited in our internal serialized inventory
                        productItem.Status = Entities.Models.Enums.ProductItemStatus.Exited;
                        productItem.RecipientName = recipientName;
                        productItem.Place = place;
                        productItem.ExitDate = exitDate;
                        productItem.Notes = string.IsNullOrWhiteSpace(productItem.Notes) 
                            ? "Exited via Excel compass import. " + notes 
                            : productItem.Notes + ". " + notes;

                        await _repositoryWrapper.ProductItems.Update(productItem.Id.ToString(), new ProductItemUpdateDto
                        {
                            Id = productItem.Id,
                            ProductId = productItem.ProductId,
                            SerialNumber = productItem.SerialNumber,
                            QRCode = productItem.QRCode,
                            Status = productItem.Status,
                            RecipientName = productItem.RecipientName,
                            Place = productItem.Place,
                            ExitDate = productItem.ExitDate,
                            Notes = productItem.Notes
                        });

                        // Deduct product inventory count
                        var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(productItem.ProductId);
                        if (inventory != null)
                        {
                            inventory.Quantity = Math.Max(0, inventory.Quantity - 1);
                            await _repositoryWrapper.Inventories.Update(inventory.Id.ToString(), new Contracts.DTOs.Inventory.InventoryUpdateDto
                            {
                                Id = inventory.Id,
                                ProductId = inventory.ProductId,
                                Quantity = inventory.Quantity,
                                MinStock = inventory.MinStock,
                                MaxStock = inventory.MaxStock
                            });
                        }
                        updatedInventoryCount++;
                    }
                }
            }

            return Ok(new SingleObjectResponseModel
            {
                IsDone = true,
                ReturnMessage = $"Successfully imported {importedCount} records to Compass. Updated {updatedInventoryCount} active inventory items status to Exited."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"An error occurred during Excel import: {ex.Message}"
            });
        }
    }
}
