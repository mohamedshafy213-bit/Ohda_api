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

    [HttpGet("export/template/products")]
    [Authorize(Roles = "Admin, Manager")]
    public IActionResult ExportProductTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Product Template");

        // Headers
        worksheet.Cell(1, 1).Value = "Product Name";
        worksheet.Cell(1, 2).Value = "SKU";
        worksheet.Cell(1, 3).Value = "Barcode";
        worksheet.Cell(1, 4).Value = "Category Name";
        worksheet.Cell(1, 5).Value = "Supplier Name";
        worksheet.Cell(1, 6).Value = "Unit Price";
        worksheet.Cell(1, 7).Value = "Purchase Price";
        worksheet.Cell(1, 8).Value = "Inventory Type (Purchased/Owned)";
        worksheet.Cell(1, 9).Value = "Initial Quantity";
        worksheet.Cell(1, 10).Value = "Min Stock";
        worksheet.Cell(1, 11).Value = "Max Stock";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.Navy;
        headerRow.Style.Font.SetFontColor(XLColor.White);

        // Add a helper comment or tip row
        worksheet.Cell(2, 1).Value = "Sample Product (Delete this row)";
        worksheet.Cell(2, 2).Value = "SMPL-SKU-001";
        worksheet.Cell(2, 3).Value = "123456789012";
        worksheet.Cell(2, 4).Value = "Electronics";
        worksheet.Cell(2, 5).Value = "TechSupplier Co";
        worksheet.Cell(2, 6).Value = 99.99m;
        worksheet.Cell(2, 7).Value = 75.00m;
        worksheet.Cell(2, 8).Value = "Purchased";
        worksheet.Cell(2, 9).Value = 20;
        worksheet.Cell(2, 10).Value = 5;
        worksheet.Cell(2, 11).Value = 100;

        worksheet.Row(2).Style.Font.Italic = true;
        worksheet.Row(2).Style.Font.SetFontColor(XLColor.Gray);

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Product_Import_Template.xlsx");
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
        int updatedCount = 0;
        int failedCount = 0;

        foreach (var row in rows)
        {
            try
            {
                string name = row.Cell(1).GetValue<string>();
                string sku = row.Cell(2).GetValue<string>();
                string barcode = row.Cell(3).GetValue<string>();
                string categoryName = row.Cell(4).GetValue<string>();
                string supplierName = row.Cell(5).GetValue<string>();
                decimal unitPrice = row.Cell(6).GetValue<decimal>();
                decimal purchasePrice = row.Cell(7).GetValue<decimal>();
                string invTypeStr = row.Cell(8).GetValue<string>();
                int initialQty = row.Cell(9).GetValue<int>();
                int minStock = row.Cell(10).GetValue<int>();
                int maxStock = row.Cell(11).GetValue<int>();

                // Skip the sample instruction row or empty rows
                if (string.IsNullOrWhiteSpace(name) || name.Contains("Sample Product") || string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(barcode))
                    continue;

                // Resolve Category (find or create)
                int categoryId;
                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    var cat = await _repositoryWrapper.Categories.GetByNameAsync(categoryName);
                    if (cat == null)
                    {
                        var createResponse = await _repositoryWrapper.Categories.Create(new Contracts.DTOs.Category.CategoryCreateDto
                        {
                            Name = categoryName,
                            Description = "Auto-created during Excel import"
                        });
                        var newCat = (createResponse as SingleObjectResponseModel<Contracts.DTOs.Category.CategoryDto>)?.SingleObject;
                        categoryId = newCat?.Id ?? 1;
                    }
                    else
                    {
                        categoryId = cat.Id;
                    }
                }
                else
                {
                    var categoriesResponse = await _repositoryWrapper.Categories.FindAll();
                    var categoriesList = (categoriesResponse as ListOfObjectsResponseModel<Contracts.DTOs.Category.CategoryDto>)?.Objects;
                    categoryId = categoriesList?.FirstOrDefault()?.Id ?? 1;
                }

                // Resolve Supplier (find or create)
                int supplierId;
                if (!string.IsNullOrWhiteSpace(supplierName))
                {
                    var sup = await _repositoryWrapper.Suppliers.GetByNameAsync(supplierName);
                    if (sup == null)
                    {
                        var createResponse = await _repositoryWrapper.Suppliers.Create(new Contracts.DTOs.Supplier.SupplierCreateDto
                        {
                            CompanyName = supplierName,
                            ContactName = "Import Agent",
                            Email = "imported@example.com",
                            Phone = "0000"
                        });
                        var newSup = (createResponse as SingleObjectResponseModel<Contracts.DTOs.Supplier.SupplierDto>)?.SingleObject;
                        supplierId = newSup?.Id ?? 1;
                    }
                    else
                    {
                        supplierId = sup.Id;
                    }
                }
                else
                {
                    var suppliersResponse = await _repositoryWrapper.Suppliers.FindAll();
                    var suppliersList = (suppliersResponse as ListOfObjectsResponseModel<Contracts.DTOs.Supplier.SupplierDto>)?.Objects;
                    supplierId = suppliersList?.FirstOrDefault()?.Id ?? 1;
                }

                // Resolve Inventory Type
                var inventoryType = Entities.Models.Enums.InventoryType.Purchased;
                if (!string.IsNullOrWhiteSpace(invTypeStr) && invTypeStr.Equals("Owned", StringComparison.OrdinalIgnoreCase))
                {
                    inventoryType = Entities.Models.Enums.InventoryType.Owned;
                }

                // Resolve Product
                var existing = await _repositoryWrapper.Products.GetByBarcodeAsync(barcode);
                int productId;
                if (existing == null)
                {
                    var createResponse = await _repositoryWrapper.Products.Create(new Contracts.DTOs.Product.ProductCreateDto
                    {
                        Name = name,
                        SKU = sku,
                        Barcode = barcode,
                        UnitPrice = unitPrice > 0 ? unitPrice : 10.00m,
                        PurchasePrice = purchasePrice > 0 ? purchasePrice : 0.00m,
                        CategoryId = categoryId,
                        SupplierId = supplierId,
                        InventoryType = inventoryType
                    });

                    var newProduct = (createResponse as SingleObjectResponseModel<Contracts.DTOs.Product.ProductDto>)?.SingleObject;
                    if (newProduct != null)
                    {
                        productId = newProduct.Id;
                        importedCount++;

                        // Seed initial inventory stock for the new product
                        var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(productId);
                        if (inventory == null)
                        {
                            await _repositoryWrapper.Inventories.Create(new Contracts.DTOs.Inventory.InventoryCreateDto
                            {
                                ProductId = productId,
                                Quantity = initialQty >= 0 ? initialQty : 0,
                                MinStock = minStock > 0 ? minStock : 5,
                                MaxStock = maxStock > 0 ? maxStock : 100
                            });
                        }
                    }
                }
                else
                {
                    productId = existing.Id;
                    updatedCount++;

                    // Update existing inventory quantity (increment it)
                    var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(productId);
                    if (inventory != null)
                    {
                        int newStock = inventory.Quantity + (initialQty >= 0 ? initialQty : 0);
                        await _repositoryWrapper.Inventories.Update(inventory.Id.ToString(), new Contracts.DTOs.Inventory.InventoryUpdateDto
                        {
                            Id = inventory.Id,
                            ProductId = productId,
                            Quantity = newStock,
                            MinStock = minStock > 0 ? minStock : inventory.MinStock,
                            MaxStock = maxStock > 0 ? maxStock : inventory.MaxStock
                        });
                    }
                    else
                    {
                        await _repositoryWrapper.Inventories.Create(new Contracts.DTOs.Inventory.InventoryCreateDto
                        {
                            ProductId = productId,
                            Quantity = initialQty >= 0 ? initialQty : 0,
                            MinStock = minStock > 0 ? minStock : 5,
                            MaxStock = maxStock > 0 ? maxStock : 100
                        });
                    }
                }
            }
            catch (Exception)
            {
                failedCount++;
            }
        }

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = $"Excel processing complete: Imported {importedCount} new products, updated {updatedCount} existing products, and {failedCount} rows failed."
        });
    }
}
