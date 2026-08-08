using Contracts.DTOs.Inventory;
using Contracts.DTOs.ScanTransaction;
using Contracts.DTOs.Category;
using Contracts.DTOs.Product;
using Contracts.DTOs.ProductItem;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;
using UglyToad.PdfPig;
using System.Text.RegularExpressions;


namespace Service_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InventoryController : BaseController<Inventory, InventoryDto, InventoryCreateDto, InventoryUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public InventoryController(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.Inventories;
    }

    [HttpPost("scan")]
    [Authorize(Roles = "Admin, Employee")]
    public async Task<IActionResult> ScanBarcode([FromBody] ScanRequestDto scanRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(scanRequest.BarcodeScanned))
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "BarcodeScanned is required."
            });
        }

        var product = await _repositoryWrapper.Products.GetByBarcodeAsync(scanRequest.BarcodeScanned);
        if (product == null)
        {
            return Ok(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"No product found for scanned barcode '{scanRequest.BarcodeScanned}'."
            });
        }

        // Log Scan Transaction
        await _repositoryWrapper.ScanTransactions.Create(new ScanTransactionCreateDto
        {
            BarcodeScanned = scanRequest.BarcodeScanned,
            TransactionType = scanRequest.TransactionType,
            Quantity = scanRequest.Quantity,
            ScannerDeviceId = scanRequest.ScannerDeviceId,
            ProductId = product.Id
        });

        // Get or Create Inventory for Product
        var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(product.Id);
        if (inventory == null)
        {
            await _repositoryWrapper.Inventories.Create(new InventoryCreateDto
            {
                ProductId = product.Id,
                Quantity = scanRequest.TransactionType switch
                {
                    TransactionType.StockIn => scanRequest.Quantity,
                    TransactionType.Audit => scanRequest.Quantity,
                    _ => 0
                },
                MinStock = 5,
                MaxStock = 100
            });
        }
        else
        {
            int updatedQuantity = inventory.Quantity;
            switch (scanRequest.TransactionType)
            {
                case TransactionType.StockIn:
                    updatedQuantity += scanRequest.Quantity;
                    break;
                case TransactionType.StockOut:
                    updatedQuantity = Math.Max(0, updatedQuantity - scanRequest.Quantity);
                    break;
                case TransactionType.Audit:
                    updatedQuantity = scanRequest.Quantity;
                    break;
            }

            await _repositoryWrapper.Inventories.Update(inventory.Id, new InventoryUpdateDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                Quantity = updatedQuantity,
                MinStock = inventory.MinStock,
                MaxStock = inventory.MaxStock
            });
        }

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = $"Barcode scanned successfully. Transaction logged and inventory updated for '{product.Name}'."
        });
    }

    [HttpPost("import/pdf")]
    [Authorize(Roles = "Admin, Manager")]
    public async Task<IActionResult> ImportPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Please select a valid PDF file."
            });
        }

        try
        {
            using var stream = file.OpenReadStream();
            using var document = PdfDocument.Open(stream);
            
            var fullTextBuilder = new System.Text.StringBuilder();
            for (int i = 1; i <= document.NumberOfPages; i++)
            {
                var page = document.GetPage(i);
                fullTextBuilder.AppendLine($"--- PAGE {i} ---");
                fullTextBuilder.AppendLine(page.Text);
            }
            string fullText = fullTextBuilder.ToString();

            string[] pages = fullText.Split(new[] { "--- PAGE " }, StringSplitOptions.None);
            var allItems = new List<ParsedItem>();

            int expectedIndex = 1;

            for (int pIdx = 1; pIdx < pages.Length; pIdx++)
            {
                string pageText = pages[pIdx];
                int headerEnd = pageText.IndexOf("---");
                if (headerEnd >= 0) pageText = pageText.Substring(headerEnd + 3);

                while (true)
                {
                    string arabicCurrent = ConvertToArabicDigits(expectedIndex);
                    string arabicNext = ConvertToArabicDigits(expectedIndex + 1);

                    string currentDelimiter = "--" + arabicCurrent;
                    string nextDelimiter = "--" + arabicNext;

                    int currentPos = pageText.IndexOf(currentDelimiter);
                    if (currentPos < 0)
                    {
                        int nextPos = pageText.IndexOf(nextDelimiter);
                        if (nextPos >= 0)
                        {
                            string itemBlock = pageText.Substring(0, nextPos);
                            ParseAndAddItem(expectedIndex, itemBlock, allItems);
                            pageText = pageText.Substring(nextPos);
                            expectedIndex++;
                            continue;
                        }
                        else break;
                    }

                    int nextDelimiterPos = pageText.IndexOf(nextDelimiter);
                    if (nextDelimiterPos >= 0)
                    {
                        string itemBlock = pageText.Substring(currentPos, nextDelimiterPos - currentPos);
                        ParseAndAddItem(expectedIndex, itemBlock, allItems);
                        pageText = pageText.Substring(nextDelimiterPos);
                        expectedIndex++;
                    }
                    else
                    {
                        int signaturePos = pageText.IndexOf("عريف / ابراهيم جمعة");
                        if (signaturePos < 0) signaturePos = pageText.IndexOf("الطرفين");
                        if (signaturePos < 0) signaturePos = pageText.IndexOf("توقيع");
                        
                        string itemBlock = signaturePos >= 0 
                            ? pageText.Substring(currentPos, signaturePos - currentPos)
                            : pageText.Substring(currentPos);
                            
                        ParseAndAddItem(expectedIndex, itemBlock, allItems);
                        expectedIndex++;
                        break;
                    }
                }
            }

            int createdProductsCount = 0;
            int updatedInventoriesCount = 0;
            int generatedItemsCount = 0;

            var suppliersResponse = await _repositoryWrapper.Suppliers.FindAll();
            var supplierDtos = (suppliersResponse as SingleObjectResponseModel<List<Contracts.DTOs.Supplier.SupplierDto>>)?.SingleObject ?? new();
            int defaultSupplierId = supplierDtos.FirstOrDefault()?.Id ?? 1;

            foreach (var item in allItems)
            {
                if (string.IsNullOrWhiteSpace(item.Name) || item.Quantity <= 0)
                    continue;

                var categoryResponse = await _repositoryWrapper.Categories.FindAll();
                var categoryDtos = (categoryResponse as SingleObjectResponseModel<List<CategoryDto>>)?.SingleObject ?? new();
                var category = categoryDtos.FirstOrDefault(c => c.Name.Equals(item.Category, StringComparison.OrdinalIgnoreCase));
                int categoryId;
                if (category == null)
                {
                    var catResponse = await _repositoryWrapper.Categories.Create(new CategoryCreateDto
                    {
                        Name = item.Category,
                        Description = "Imported from custody PDF"
                    });
                    categoryId = ((catResponse as SingleObjectResponseModel<CategoryDto>)?.SingleObject)?.Id ?? 1;
                }
                else
                {
                    categoryId = category.Id;
                }

                var product = await _repositoryWrapper.Products.GetByNameAsync(item.Name);
                int productId;
                string barcode;
                if (product == null)
                {
                    string cleanPage = item.Page.Replace("/", "-");
                    barcode = $"BAR-PDF-{cleanPage}-{item.Index}";
                    string sku = $"SKU-PDF-{cleanPage}-{item.Index}";

                    var prodResponse = await _repositoryWrapper.Products.Create(new ProductCreateDto
                    {
                        Name = item.Name,
                        SKU = sku,
                        Barcode = barcode,
                        CategoryId = categoryId,
                        SupplierId = defaultSupplierId,
                        UnitPrice = 10.00m,
                        InventoryType = InventoryType.Owned,
                        PurchasePrice = 0.00m,
                        AssetValue = 0.00m
                    });
                    productId = ((prodResponse as SingleObjectResponseModel<ProductDto>)?.SingleObject)?.Id ?? 1;
                    createdProductsCount++;
                }
                else
                {
                    productId = product.Id;
                    barcode = product.Barcode;
                }

                var inventory = await _repositoryWrapper.Inventories.GetByProductIdAsync(productId);
                if (inventory == null)
                {
                    await _repositoryWrapper.Inventories.Create(new InventoryCreateDto
                    {
                        ProductId = productId,
                        Quantity = item.Quantity,
                        MinStock = 5,
                        MaxStock = 500
                    });
                }
                else
                {
                    inventory.Quantity += item.Quantity;
                    await _repositoryWrapper.Inventories.Update(inventory.Id.ToString(), new InventoryUpdateDto
                    {
                        Id = inventory.Id,
                        ProductId = inventory.ProductId,
                        Quantity = inventory.Quantity,
                        MinStock = inventory.MinStock,
                        MaxStock = inventory.MaxStock
                    });
                }
                updatedInventoriesCount += item.Quantity;

                for (int u = 1; u <= item.Quantity; u++)
                {
                    string guidSuffix = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                    string serialNumber = $"SN-{barcode}-{u}-{guidSuffix}";
                    string qrCode = $"QR-{serialNumber}";

                    await _repositoryWrapper.ProductItems.Create(new ProductItemCreateDto
                    {
                        ProductId = productId,
                        SerialNumber = serialNumber,
                        QRCode = qrCode,
                        Status = ProductItemStatus.InStock
                    });
                    generatedItemsCount++;
                }
            }

            await _repositoryWrapper.SaveAsync();

            return Ok(new SingleObjectResponseModel
            {
                IsDone = true,
                ReturnMessage = $"PDF import completed. Created {createdProductsCount} products, updated stock for {updatedInventoriesCount} items, and generated {generatedItemsCount} serialized device items."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = $"An error occurred during PDF import: {ex.Message}"
            });
        }
    }

    private class ParsedItem
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Page { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    private static void ParseAndAddItem(int index, string block, List<ParsedItem> list)
    {
        string engBlock = ConvertArabicDigitsToEnglish(block);
        string delim = "--" + index;
        int delimIdx = engBlock.IndexOf(delim);
        if (delimIdx >= 0)
        {
            engBlock = engBlock.Substring(delimIdx + delim.Length);
            block = block.Substring(delimIdx + delim.Length);
        }

        engBlock = engBlock.Trim();
        block = block.Trim();

        var digitsMatch = Regex.Match(engBlock, @"^[0-9/]+");
        
        string page = "N/A";
        int qty = 1;
        string cleanedName = block;

        if (digitsMatch.Success)
        {
            string digitStr = digitsMatch.Value;
            cleanedName = block.Substring(digitStr.Length).Trim();

            var pageMatch = Regex.Match(digitStr, @"(\d{1,3}/\d+)$");
            if (pageMatch.Success)
            {
                page = pageMatch.Value;
                string prefix = digitStr.Substring(0, digitStr.Length - page.Length);
                qty = ExtractQuantity(prefix);
            }
            else
            {
                qty = ExtractQuantity(digitStr);
            }
        }

        string name = cleanedName;
        string category = "General";
        int slashIdx = cleanedName.LastIndexOf('/');
        if (slashIdx >= 0)
        {
            name = cleanedName.Substring(0, slashIdx).Trim();
            category = cleanedName.Substring(slashIdx + 1).Trim();
        }

        list.Add(new ParsedItem
        {
            Index = index,
            Name = CleanReversedText(name),
            Category = CleanReversedText(category),
            Page = page,
            Quantity = qty
        });
    }

    private static int ExtractQuantity(string digits)
    {
        if (string.IsNullOrEmpty(digits)) return 1;
        var zeroMatch = Regex.Match(digits, @"0{2,}");
        if (zeroMatch.Success)
        {
            string qtyStr = digits.Substring(0, zeroMatch.Index);
            if (int.TryParse(qtyStr, out int qty)) return qty;
        }

        if (int.TryParse(digits, out int fallbackQty)) return fallbackQty;
        return 1;
    }

    private static string CleanReversedText(string text)
    {
        text = text.Replace("--", "").Trim();
        return text;
    }

    private static string ConvertArabicDigitsToEnglish(string input)
    {
        char[] arabicDigits = { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };
        for (int i = 0; i < 10; i++)
        {
            input = input.Replace(arabicDigits[i], (char)('0' + i));
        }
        return input;
    }

    private static string ConvertToArabicDigits(int value)
    {
        string valStr = value.ToString();
        char[] arabicDigits = { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in valStr)
        {
            if (char.IsDigit(c)) sb.Append(arabicDigits[c - '0']);
            else sb.Append(c);
        }
        return sb.ToString();
    }
}

