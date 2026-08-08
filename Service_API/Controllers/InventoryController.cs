using Contracts.DTOs.Inventory;
using Contracts.DTOs.ScanTransaction;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;

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
}
