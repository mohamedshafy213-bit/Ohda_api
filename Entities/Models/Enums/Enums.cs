namespace Entities.Models.Enums;

public enum UserRole
{
    Admin = 1,
    Employee = 2,
    Supervisor = 3,
    Manager = 4
}

public enum InventoryType
{
    Purchased = 1,
    Owned = 2
}

public enum TransactionType
{
    StockIn = 1,
    StockOut = 2,
    Audit = 3
}

public enum RequestStatus
{
    Pending = 1,
    SupervisorApproved = 2,
    ManagerApproved = 3,
    Rejected = 4
}

public enum NotificationType
{
    Info = 1,
    Warning = 2,
    ExitRequest = 3,
    StockAlert = 4,
    EntryRequest = 5
}
