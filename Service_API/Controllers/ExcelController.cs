using ClosedXML.Excel;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExcelController : ControllerBase
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public ExcelController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
    }

    [HttpGet("export/products")]
    [Authorize(Roles = "Admin, Manager, Employee, Supervisor")]
    public async Task<IActionResult> ExportProducts()
    {
        var response = await _repositoryWrapper.Products.FindAll();
        var products = (response as SingleObjectResponseModel<List<Contracts.DTOs.Product.ProductDto>>)?.SingleObject ?? new();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Products");

        // Header
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "SKU";
        worksheet.Cell(1, 4).Value = "Barcode";
        worksheet.Cell(1, 5).Value = "Category Name";
        worksheet.Cell(1, 6).Value = "Supplier Name";
        worksheet.Cell(1, 7).Value = "Unit Price";
        worksheet.Cell(1, 8).Value = "Purchase Price";
        worksheet.Cell(1, 9).Value = "Inventory Type";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.Navy;
        headerRow.Style.Font.SetFontColor(XLColor.White);

        int row = 2;
        foreach (var p in products)
        {
            worksheet.Cell(row, 1).Value = p.Id;
            worksheet.Cell(row, 2).Value = p.Name;
            worksheet.Cell(row, 3).Value = p.SKU;
            worksheet.Cell(row, 4).Value = p.Barcode;
            worksheet.Cell(row, 5).Value = p.CategoryName ?? "";
            worksheet.Cell(row, 6).Value = p.SupplierName ?? "";
            worksheet.Cell(row, 7).Value = p.UnitPrice;
            worksheet.Cell(row, 8).Value = p.PurchasePrice ?? 0;
            worksheet.Cell(row, 9).Value = p.InventoryType.ToString();
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products_Export.xlsx");
    }

    [HttpGet("export/categories")]
    [Authorize(Roles = "Admin, Manager, Employee, Supervisor")]
    public async Task<IActionResult> ExportCategories()
    {
        var response = await _repositoryWrapper.Categories.FindAll();
        var categories = (response as SingleObjectResponseModel<List<Contracts.DTOs.Category.CategoryDto>>)?.SingleObject ?? new();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Categories");

        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Description";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.Navy;
        headerRow.Style.Font.SetFontColor(XLColor.White);

        int row = 2;
        foreach (var c in categories)
        {
            worksheet.Cell(row, 1).Value = c.Id;
            worksheet.Cell(row, 2).Value = c.Name;
            worksheet.Cell(row, 3).Value = c.Description ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Categories_Export.xlsx");
    }

    [HttpGet("export/suppliers")]
    [Authorize(Roles = "Admin, Manager, Employee, Supervisor")]
    public async Task<IActionResult> ExportSuppliers()
    {
        var response = await _repositoryWrapper.Suppliers.FindAll();
        var suppliers = (response as SingleObjectResponseModel<List<Contracts.DTOs.Supplier.SupplierDto>>)?.SingleObject ?? new();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Suppliers");

        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Company Name";
        worksheet.Cell(1, 3).Value = "Contact Name";
        worksheet.Cell(1, 4).Value = "Phone";
        worksheet.Cell(1, 5).Value = "Email";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.Navy;
        headerRow.Style.Font.SetFontColor(XLColor.White);

        int row = 2;
        foreach (var s in suppliers)
        {
            worksheet.Cell(row, 1).Value = s.Id;
            worksheet.Cell(row, 2).Value = s.CompanyName;
            worksheet.Cell(row, 3).Value = s.ContactName ?? "";
            worksheet.Cell(row, 4).Value = s.Phone ?? "";
            worksheet.Cell(row, 5).Value = s.Email ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Suppliers_Export.xlsx");
    }

    [HttpGet("export/inventory")]
    [Authorize(Roles = "Admin, Manager, Employee, Supervisor")]
    public async Task<IActionResult> ExportInventory()
    {
        var response = await _repositoryWrapper.Inventories.FindAll();
        var inventories = (response as SingleObjectResponseModel<List<Contracts.DTOs.Inventory.InventoryDto>>)?.SingleObject ?? new();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Inventory Stock");

        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Product Name";
        worksheet.Cell(1, 3).Value = "SKU";
        worksheet.Cell(1, 4).Value = "Barcode";
        worksheet.Cell(1, 5).Value = "Quantity";
        worksheet.Cell(1, 6).Value = "Min Stock";
        worksheet.Cell(1, 7).Value = "Max Stock";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.Navy;
        headerRow.Style.Font.SetFontColor(XLColor.White);

        int row = 2;
        foreach (var i in inventories)
        {
            worksheet.Cell(row, 1).Value = i.Id;
            worksheet.Cell(row, 2).Value = i.ProductName ?? "";
            worksheet.Cell(row, 3).Value = i.ProductSKU ?? "";
            worksheet.Cell(row, 4).Value = i.ProductBarcode ?? "";
            worksheet.Cell(row, 5).Value = i.Quantity;
            worksheet.Cell(row, 6).Value = i.MinStock;
            worksheet.Cell(row, 7).Value = i.MaxStock;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Inventory_Stock_Export.xlsx");
    }

    [HttpPost("import/products")]
    [Authorize(Roles = "Admin, Manager")]
    public async Task<IActionResult> ImportProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Please select a valid Excel file."
            });
        }

        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RowsUsed().Skip(1); // Skip header

        int importedCount = 0;
        var categoriesResponse = await _repositoryWrapper.Categories.FindAll();
        var categories = (categoriesResponse as SingleObjectResponseModel<List<Contracts.DTOs.Category.CategoryDto>>)?.SingleObject ?? new();

        var suppliersResponse = await _repositoryWrapper.Suppliers.FindAll();
        var suppliers = (suppliersResponse as SingleObjectResponseModel<List<Contracts.DTOs.Supplier.SupplierDto>>)?.SingleObject ?? new();

        int defaultCategoryId = categories.FirstOrDefault()?.Id ?? 1;
        int defaultSupplierId = suppliers.FirstOrDefault()?.Id ?? 1;

        foreach (var row in rows)
        {
            string name = row.Cell(1).GetValue<string>();
            string sku = row.Cell(2).GetValue<string>();
            string barcode = row.Cell(3).GetValue<string>();
            decimal unitPrice = row.Cell(4).GetValue<decimal>();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(barcode))
                continue;

            var existing = await _repositoryWrapper.Products.GetByBarcodeAsync(barcode);
            if (existing == null)
            {
                await _repositoryWrapper.Products.Create(new Contracts.DTOs.Product.ProductCreateDto
                {
                    Name = name,
                    SKU = sku,
                    Barcode = barcode,
                    UnitPrice = unitPrice > 0 ? unitPrice : 10.00m,
                    CategoryId = defaultCategoryId,
                    SupplierId = defaultSupplierId,
                    InventoryType = Entities.Models.Enums.InventoryType.Purchased
                });
                importedCount++;
            }
        }

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = $"Successfully imported {importedCount} new products from Excel."
        });
    }
}
