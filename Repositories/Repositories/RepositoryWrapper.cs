using Contracts.interfaces.Repository;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Models;

namespace Repositories.Repositories;

public class RepositoryWrapper : IRepositoryWrapper
{
    private readonly RepositoryContext _repoContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly ILoggerManager _logger;

    private IUserRepository? _users;
    private ICategoryRepository? _categories;
    private ISupplierRepository? _suppliers;
    private IProductRepository? _products;
    private IInventoryRepository? _inventories;
    private IOrderRepository? _orders;
    private IScanTransactionRepository? _scanTransactions;
    private IProductExitRequestRepository? _productExitRequests;
    private IProductEntryRequestRepository? _productEntryRequests;
    private IPageRepository? _pages;
    private IUserPagePermissionRepository? _userPagePermissions;
    private INotificationRepository? _notifications;
    private IUserGroupRepository? _userGroups;
    private IGroupPagePermissionRepository? _groupPagePermissions;
    private IProductItemRepository? _productItems;
    private ICompassRepository? _compasses;
    private IApprovalConfigRepository? _approvalConfigs;
    private IDepartmentRepository? _departments;
    private IProductStateRepository? _productStates;


    public RepositoryWrapper(
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        ILoggerManager logger)
    {
        _repoContext = repositoryContext;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _logger = logger;
    }

    public IUserRepository Users =>
        _users ??= new UserRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public ICategoryRepository Categories =>
        _categories ??= new CategoryRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public ISupplierRepository Suppliers =>
        _suppliers ??= new SupplierRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IProductRepository Products =>
        _products ??= new ProductRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IInventoryRepository Inventories =>
        _inventories ??= new InventoryRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IOrderRepository Orders =>
        _orders ??= new OrderRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IScanTransactionRepository ScanTransactions =>
        _scanTransactions ??= new ScanTransactionRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IProductExitRequestRepository ProductExitRequests =>
        _productExitRequests ??= new ProductExitRequestRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IProductEntryRequestRepository ProductEntryRequests =>
        _productEntryRequests ??= new ProductEntryRequestRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IPageRepository Pages =>
        _pages ??= new PageRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IUserPagePermissionRepository UserPagePermissions =>
        _userPagePermissions ??= new UserPagePermissionRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public INotificationRepository Notifications =>
        _notifications ??= new NotificationRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IUserGroupRepository UserGroups =>
        _userGroups ??= new UserGroupRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IGroupPagePermissionRepository GroupPagePermissions =>
        _groupPagePermissions ??= new GroupPagePermissionRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IProductItemRepository ProductItems =>
        _productItems ??= new ProductItemRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public ICompassRepository Compasses =>
        _compasses ??= new CompassRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IApprovalConfigRepository ApprovalConfigs =>
        _approvalConfigs ??= new ApprovalConfigRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IDepartmentRepository Departments =>
        _departments ??= new DepartmentRepository(_logger, _repoContext, _httpContextAccessor, _mapper);

    public IProductStateRepository ProductStates =>
        _productStates ??= new ProductStateRepository(_logger, _repoContext, _httpContextAccessor, _mapper);


    public void Save()
    {
        _repoContext.SaveChanges();
    }

    public async Task SaveAsync()
    {
        await _repoContext.SaveChangesAsync();
    }
}
