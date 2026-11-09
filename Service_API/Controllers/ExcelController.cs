using ClosedXML.Excel;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Databases;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExcelController : ControllerBase
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly RepositoryContext _context;

    public ExcelController(IRepositoryWrapper repositoryWrapper, RepositoryContext context)
    {
        _repositoryWrapper = repositoryWrapper;
        _context = context;
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
    [Authorize]
    public IActionResult ExportProductTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Product Template");

        // Headers with * for required fields
        worksheet.Cell(1, 1).Value = "اسم الصنف / Product Name *";
        worksheet.Cell(1, 2).Value = "التصنيف / Category Name *";
        worksheet.Cell(1, 3).Value = "الرقم المسلسل للجهاز / Device Serial (S/N) (يفصل بفاصلة عند تعدد الأجهزة)";
        worksheet.Cell(1, 4).Value = "رمز الصنف / SKU";
        worksheet.Cell(1, 5).Value = "الباركود / Barcode";
        worksheet.Cell(1, 6).Value = "اسم المورد / Supplier Name";
        worksheet.Cell(1, 7).Value = "سعر الشراء / Purchase Price";
        worksheet.Cell(1, 8).Value = "سعر البيع / Unit Price";
        worksheet.Cell(1, 9).Value = "نوع المخزون / Inventory Type (Purchased/Owned)";
        worksheet.Cell(1, 10).Value = "الكمية / Quantity";
        worksheet.Cell(1, 11).Value = "الحد الأدنى / Min Stock";
        worksheet.Cell(1, 12).Value = "الحد الأقصى / Max Stock";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(15, 76, 129); // Professional Navy
        headerRow.Style.Font.SetFontColor(XLColor.White);
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        headerRow.Height = 32;

        // Sample Row 1: Desktop PC with Device Serial Number
        worksheet.Cell(2, 1).Value = "جهاز حاسب آلي مكتبي Dell OptiPlex 7090";
        worksheet.Cell(2, 2).Value = "أجهزة حاسب آلي";
        worksheet.Cell(2, 3).Value = "SN-DELL-884920";
        worksheet.Cell(2, 4).Value = "DELL-OPT-7090";
        worksheet.Cell(2, 5).Value = "6281001001";
        worksheet.Cell(2, 6).Value = "شركة تقنية النظم";
        worksheet.Cell(2, 7).Value = 3500;
        worksheet.Cell(2, 8).Value = 4200;
        worksheet.Cell(2, 9).Value = "Purchased";
        worksheet.Cell(2, 10).Value = 1;
        worksheet.Cell(2, 11).Value = 5;
        worksheet.Cell(2, 12).Value = 50;

        // Sample Row 2: Monitor with Device Serial Number
        worksheet.Cell(3, 1).Value = "شاشة عرض HP 24 بوصة FHD";
        worksheet.Cell(3, 2).Value = "شاشات وعوارض";
        worksheet.Cell(3, 3).Value = "SN-HP-24-9912";
        worksheet.Cell(3, 4).Value = "HP-ED-24";
        worksheet.Cell(3, 5).Value = "6281001002";
        worksheet.Cell(3, 6).Value = "شركة تقنية النظم";
        worksheet.Cell(3, 7).Value = 750;
        worksheet.Cell(3, 8).Value = 900;
        worksheet.Cell(3, 9).Value = "Purchased";
        worksheet.Cell(3, 10).Value = 1;
        worksheet.Cell(3, 11).Value = 5;
        worksheet.Cell(3, 12).Value = 50;

        // Sample Row 3: Laptops with 3 distinct comma-separated serial numbers
        worksheet.Cell(4, 1).Value = "كمبيوتر محمول Lenovo ThinkPad L14";
        worksheet.Cell(4, 2).Value = "أجهزة حاسب آلي";
        worksheet.Cell(4, 3).Value = "SN-LEN-881, SN-LEN-882, SN-LEN-883";
        worksheet.Cell(4, 4).Value = "LEN-TP-L14";
        worksheet.Cell(4, 5).Value = "6281001003";
        worksheet.Cell(4, 6).Value = "شركة تقنية النظم";
        worksheet.Cell(4, 7).Value = 2800;
        worksheet.Cell(4, 8).Value = 3300;
        worksheet.Cell(4, 9).Value = "Purchased";
        worksheet.Cell(4, 10).Value = 3;
        worksheet.Cell(4, 11).Value = 5;
        worksheet.Cell(4, 12).Value = 50;

        // Light background for sample rows to distinguish them
        var sampleRange = worksheet.Range(2, 1, 4, 12);
        sampleRange.Style.Fill.BackgroundColor = XLColor.FromArgb(245, 247, 250);
        sampleRange.Style.Font.SetFontColor(XLColor.FromArgb(70, 80, 95));

        worksheet.RightToLeft = true;
        worksheet.SheetView.FreezeRows(1);
        worksheet.Columns().AdjustToContents(18.0, 45.0);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products_Import_Template.xlsx");
    }

    [HttpPost("import/products")]
    [Authorize]
    public async Task<IActionResult> ImportProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "يرجى اختيار ملف Excel صحيح."
            });
        }

        var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
        int branchId = (_context.CurrentBranchId.HasValue && _context.CurrentBranchId.Value > 0)
            ? _context.CurrentBranchId.Value
            : (int.TryParse(branchClaim, out var bId) && bId > 0 ? bId : 1);
        var userCode = User.FindFirst(ClaimTypes.Name)?.Value ?? "ImportAgent";

        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RowsUsed().ToList();

        if (rows.Count <= 1)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "ملف Excel فارغ ولا يحتوي على صفوف بيانات."
            });
        }

        // Dynamically find header row (typically row 1, or row 2 if title row exists)
        int headerRowIndex = 0;
        for (int r = 0; r < Math.Min(5, rows.Count); r++)
        {
            var text = string.Join(" ", rows[r].CellsUsed().Select(c => GetCellString(c).ToLower()));
            if (text.Contains("اسم") || text.Contains("name") || text.Contains("صنف") || text.Contains("product") || text.Contains("sku") || text.Contains("barcode"))
            {
                headerRowIndex = r;
                break;
            }
        }

        var headerRow = rows[headerRowIndex];
        int nameCol = 1, catCol = 2, serialCol = 3, skuCol = 4, barcodeCol = 5;
        int supplierCol = 6, purchaseCol = 7, unitCol = 8, typeCol = 9, qtyCol = 10;
        int minCol = 11, maxCol = 12;

        int totalColumns = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 12;
        for (int c = 1; c <= totalColumns; c++)
        {
            string h = GetCellString(headerRow.Cell(c)).ToLower();
            if (h.Contains("اسم الصنف") || h.Contains("product name") || h.Contains("اسم المنتج") || h.Contains("item name") || h.Contains("اسم الجهاز")) nameCol = c;
            else if (h.Contains("تصنيف") || h.Contains("category") || h.Contains("فئة") || h.Contains("الفئة")) catCol = c;
            else if (h.Contains("مسلسل") || h.Contains("تسلسل") || h.Contains("سيريال") || h.Contains("serial") || h.Contains("s/n") || h.Trim() == "sn" || h.Contains("رقم الجهاز") || h.Contains("أرقام مسلسلة")) serialCol = c;
            else if (h.Contains("sku") || h.Contains("رمز") || h.Contains("كود")) skuCol = c;
            else if (h.Contains("باركود") || h.Contains("barcode") || h.Contains("upc")) barcodeCol = c;
            else if (h.Contains("مورد") || h.Contains("supplier")) supplierCol = c;
            else if (h.Contains("شراء") || h.Contains("purchase") || h.Contains("cost")) purchaseCol = c;
            else if (h.Contains("بيع") || h.Contains("unit price") || h.Contains("سعر الوحدة")) unitCol = c;
            else if (h.Contains("نوع") || h.Contains("inventory type")) typeCol = c;
            else if (h.Contains("كمية") || h.Contains("كميه") || h.Contains("رصيد") || h.Contains("quantity") || h.Contains("qty")) qtyCol = c;
            else if (h.Contains("أدنى") || h.Contains("min")) minCol = c;
            else if (h.Contains("أقصى") || h.Contains("max")) maxCol = c;
        }

        int importedCount = 0;
        int updatedCount = 0;
        int serialsCount = 0;
        int failedCount = 0;

        foreach (var row in rows.Skip(headerRowIndex + 1))
        {
            try
            {
                string name = GetCellString(row.Cell(nameCol));
                if (string.IsNullOrWhiteSpace(name) || name.Contains("Product Name") || name.Contains("اسم الصنف"))
                    continue;

                string categoryName = GetCellString(row.Cell(catCol));
                string deviceSerial = GetCellString(row.Cell(serialCol));
                string sku = GetCellString(row.Cell(skuCol));
                string barcode = GetCellString(row.Cell(barcodeCol));
                string supplierName = GetCellString(row.Cell(supplierCol));

                // Auto-generate SKU if not provided
                if (string.IsNullOrWhiteSpace(sku))
                {
                    sku = $"SKU-{Guid.NewGuid().ToString()[..6].ToUpper()}";
                }

                // Auto-generate Barcode if not provided
                if (string.IsNullOrWhiteSpace(barcode))
                {
                    barcode = $"BC{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(10, 99)}";
                }

                decimal purchasePrice = 0;
                string pStr = GetCellString(row.Cell(purchaseCol));
                if (decimal.TryParse(pStr, out var pp) && pp >= 0) purchasePrice = pp;

                decimal unitPrice = 0;
                string uStr = GetCellString(row.Cell(unitCol));
                if (decimal.TryParse(uStr, out var up) && up >= 0) unitPrice = up;

                string invTypeStr = GetCellString(row.Cell(typeCol));
                var inventoryType = invTypeStr.Equals("Owned", StringComparison.OrdinalIgnoreCase)
                    ? InventoryType.Owned
                    : InventoryType.Purchased;

                int initialQty = 1;
                string qStr = GetCellString(row.Cell(qtyCol));
                if (int.TryParse(qStr, out var iq) && iq > 0) initialQty = iq;

                int minStock = 5;
                string minStr = GetCellString(row.Cell(minCol));
                if (int.TryParse(minStr, out var ms) && ms >= 0) minStock = ms;

                int maxStock = 100;
                string maxStr = GetCellString(row.Cell(maxCol));
                if (int.TryParse(maxStr, out var xs) && xs > 0) maxStock = xs;

                // 1. Resolve Category (find or auto-create directly in database)
                int categoryId;
                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    var cat = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == categoryName.Trim().ToLower() && !c.IsDeleted);

                    if (cat == null)
                    {
                        cat = new Category
                        {
                            Name = categoryName.Trim(),
                            Description = "تم إنشاؤه تلقائياً أثناء استيراد Excel",
                            InsertDate = DateTime.UtcNow,
                            InsertUserCode = userCode
                        };
                        await _context.Categories.AddAsync(cat);
                        await _context.SaveChangesAsync();
                    }
                    categoryId = cat.Id;
                }
                else
                {
                    var firstCat = await _context.Categories.FirstOrDefaultAsync(c => !c.IsDeleted);
                    if (firstCat == null)
                    {
                        firstCat = new Category
                        {
                            Name = "عام",
                            Description = "تصنيف عام تلقائي للمنتجات",
                            InsertDate = DateTime.UtcNow,
                            InsertUserCode = userCode
                        };
                        await _context.Categories.AddAsync(firstCat);
                        await _context.SaveChangesAsync();
                    }
                    categoryId = firstCat.Id;
                }

                // 2. Resolve Supplier (find or auto-create if provided)
                int? supplierId = null;
                if (!string.IsNullOrWhiteSpace(supplierName))
                {
                    var sup = await _context.Suppliers
                        .FirstOrDefaultAsync(s => s.CompanyName.Trim().ToLower() == supplierName.Trim().ToLower() && !s.IsDeleted);

                    if (sup == null)
                    {
                        sup = new Supplier
                        {
                            CompanyName = supplierName.Trim(),
                            ContactName = "وكيل توريد",
                            Email = "supplier@import.com",
                            Phone = "0000",
                            BranchId = branchId,
                            InsertDate = DateTime.UtcNow,
                            InsertUserCode = userCode
                        };
                        await _context.Suppliers.AddAsync(sup);
                        await _context.SaveChangesAsync();
                    }
                    supplierId = sup.Id;
                }

                // 3. Resolve Product: Check by Barcode or SKU
                var existing = await _context.Products.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => (p.Barcode == barcode || (p.SKU == sku && p.BranchId == branchId)) && !p.IsDeleted);

                int productId;
                if (existing == null)
                {
                    var product = new Product
                    {
                        Name = name,
                        SKU = sku,
                        Barcode = barcode,
                        CategoryId = categoryId,
                        SupplierId = supplierId,
                        UnitPrice = unitPrice > 0 ? unitPrice : (purchasePrice > 0 ? purchasePrice * 1.2m : 100m),
                        PurchasePrice = purchasePrice,
                        InventoryType = inventoryType,
                        BranchId = branchId,
                        InsertDate = DateTime.UtcNow,
                        InsertUserCode = userCode
                    };
                    await _context.Products.AddAsync(product);
                    await _context.SaveChangesAsync();

                    productId = product.Id;
                    importedCount++;
                }
                else
                {
                    productId = existing.Id;
                    existing.Name = name;
                    existing.CategoryId = categoryId;
                    if (supplierId.HasValue) existing.SupplierId = supplierId;
                    if (unitPrice > 0) existing.UnitPrice = unitPrice;
                    if (purchasePrice > 0) existing.PurchasePrice = purchasePrice;
                    existing.LastUpdate = DateTime.UtcNow;
                    existing.UpdateUserCode = userCode;
                    await _context.SaveChangesAsync();

                    updatedCount++;
                }

                // 4. Resolve Stock / Inventory record
                var inventory = await _context.Inventories.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(i => i.ProductId == productId && i.BranchId == branchId && !i.IsDeleted);

                if (inventory == null)
                {
                    inventory = new Inventory
                    {
                        ProductId = productId,
                        BranchId = branchId,
                        Quantity = initialQty,
                        MinStock = minStock,
                        MaxStock = maxStock,
                        InsertDate = DateTime.UtcNow,
                        InsertUserCode = userCode
                    };
                    await _context.Inventories.AddAsync(inventory);
                }
                else
                {
                    inventory.Quantity += initialQty;
                    inventory.LastUpdate = DateTime.UtcNow;
                    inventory.UpdateUserCode = userCode;
                }
                await _context.SaveChangesAsync();

                // 5. Track Serialized Device(s) in ProductItems
                var rawSerials = deviceSerial
                    .Split(new[] { ',', ';', '\n', '\r', '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .ToList();

                // If user provided more serial numbers than initialQty, expand quantity to match
                if (rawSerials.Count > initialQty)
                {
                    inventory.Quantity += (rawSerials.Count - initialQty);
                    initialQty = rawSerials.Count;
                    await _context.SaveChangesAsync();
                }

                int existingItemsCount = await _context.ProductItems
                    .CountAsync(pi => pi.ProductId == productId && pi.BranchId == branchId && !pi.IsDeleted);

                for (int k = 0; k < initialQty; k++)
                {
                    string sn = (k < rawSerials.Count)
                        ? rawSerials[k]
                        : $"SN-{sku}-{existingItemsCount + k + 1}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

                    var existingItem = await _context.ProductItems.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(pi => pi.SerialNumber == sn && pi.BranchId == branchId && !pi.IsDeleted);

                    if (existingItem == null)
                    {
                        var productItem = new ProductItem
                        {
                            ProductId = productId,
                            BranchId = branchId,
                            SerialNumber = sn,
                            QRCode = sn,
                            Status = ProductItemStatus.InStock,
                            InsertDate = DateTime.UtcNow,
                            InsertUserCode = userCode
                        };
                        await _context.ProductItems.AddAsync(productItem);
                        serialsCount++;
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                failedCount++;
            }
        }

        // 6. Branch-wide Serials Backfill: Ensure 100% of products in inventory have serial numbers
        try
        {
            var branchInventories = await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.BranchId == branchId && !i.IsDeleted && i.Quantity > 0)
                .ToListAsync();

            foreach (var inv in branchInventories)
            {
                int currentItemsCount = await _context.ProductItems
                    .CountAsync(pi => pi.ProductId == inv.ProductId && pi.BranchId == branchId && !pi.IsDeleted);

                if (currentItemsCount < inv.Quantity)
                {
                    int missing = inv.Quantity - currentItemsCount;
                    for (int m = 0; m < missing; m++)
                    {
                        string autoSn = $"SN-{inv.Product?.SKU ?? "ITEM"}-{currentItemsCount + m + 1}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
                        await _context.ProductItems.AddAsync(new ProductItem
                        {
                            ProductId = inv.ProductId,
                            BranchId = branchId,
                            SerialNumber = autoSn,
                            QRCode = autoSn,
                            Status = ProductItemStatus.InStock,
                            InsertDate = DateTime.UtcNow,
                            InsertUserCode = userCode
                        });
                        serialsCount++;
                    }
                }
            }
            await _context.SaveChangesAsync();
        }
        catch { }

        _repositoryWrapper.Products.InvalidateCache();
        _repositoryWrapper.Inventories.InvalidateCache();
        _repositoryWrapper.ProductItems.InvalidateCache();

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = $"اكتمل استيراد ملف Excel بنجاح: تم إضافة {importedCount} صنف جديد، وتحديث {updatedCount} صنف، وتسجيل {serialsCount} جهاز برقم مسلسل، وفشل {failedCount} سطر."
        });
    }

    [HttpPost("sync-all-serials")]
    [Authorize]
    public async Task<IActionResult> SyncAllSerials()
    {
        int branchId = (_context.CurrentBranchId.HasValue && _context.CurrentBranchId.Value > 0)
            ? _context.CurrentBranchId.Value
            : (int.TryParse(User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value, out var bId) && bId > 0 ? bId : 1);
        var userCode = User.FindFirst(ClaimTypes.Name)?.Value ?? "SyncAgent";

        var branchInventories = await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.BranchId == branchId && !i.IsDeleted && i.Quantity > 0)
            .ToListAsync();

        int generatedCount = 0;
        foreach (var inv in branchInventories)
        {
            int currentItemsCount = await _context.ProductItems
                .CountAsync(pi => pi.ProductId == inv.ProductId && pi.BranchId == branchId && !pi.IsDeleted);

            if (currentItemsCount < inv.Quantity)
            {
                int missing = inv.Quantity - currentItemsCount;
                for (int m = 0; m < missing; m++)
                {
                    string autoSn = $"SN-{inv.Product?.SKU ?? "ITEM"}-{currentItemsCount + m + 1}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
                    await _context.ProductItems.AddAsync(new ProductItem
                    {
                        ProductId = inv.ProductId,
                        BranchId = branchId,
                        SerialNumber = autoSn,
                        QRCode = autoSn,
                        Status = ProductItemStatus.InStock,
                        InsertDate = DateTime.UtcNow,
                        InsertUserCode = userCode
                    });
                    generatedCount++;
                }
            }
        }
        await _context.SaveChangesAsync();
        _repositoryWrapper.Products.InvalidateCache();
        _repositoryWrapper.Inventories.InvalidateCache();
        _repositoryWrapper.ProductItems.InvalidateCache();

        return Ok(new SingleObjectResponseModel<int>
        {
            IsDone = true,
            ReturnMessage = $"تم تدقيق ومزامنة الأرقام التسلسلية بنجاح: تم توليد وتسجيل {generatedCount} رقم تسلسلي للأصناف غير المتتبعة.",
            SingleObject = generatedCount
        });
    }

    private static string GetCellString(IXLCell cell)
    {
        if (cell == null || cell.IsEmpty()) return string.Empty;
        try
        {
            return cell.Value.ToString()?.Trim() ?? string.Empty;
        }
        catch
        {
            return cell.GetFormattedString()?.Trim() ?? string.Empty;
        }
    }
}
