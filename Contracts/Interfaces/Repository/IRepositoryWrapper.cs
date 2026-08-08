using Contracts.Interfaces.Repository;

namespace Contracts.interfaces.Repository;

public interface IRepositoryWrapper
{
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    ISupplierRepository Suppliers { get; }
    IProductRepository Products { get; }
    IInventoryRepository Inventories { get; }
    IOrderRepository Orders { get; }
    IScanTransactionRepository ScanTransactions { get; }
    IProductExitRequestRepository ProductExitRequests { get; }
    IProductEntryRequestRepository ProductEntryRequests { get; }
    IPageRepository Pages { get; }
    IUserPagePermissionRepository UserPagePermissions { get; }
    INotificationRepository Notifications { get; }

    void Save();
    Task SaveAsync();
}
