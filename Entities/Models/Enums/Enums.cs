namespace Entities.Models.Enums;

public enum UserRole
{
    SuperAdmin = 0,
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
    Rejected = 4,
    Approved = 5
}

public enum RequestType
{
    Entry = 1,
    Exit = 2
}

public enum WorkflowRole
{
    Requester = 1,
    Reviewer = 2,
    Approver = 3
}

public enum CompassType
{
    Entry = 1,
    Exit = 2
}

public enum NotificationType
{
    Info = 1,
    Warning = 2,
    ExitRequest = 3,
    StockAlert = 4,
    EntryRequest = 5
}

public enum ProductItemStatus
{
    InStock = 1,
    Exited = 2,
    Damaged = 3,
    InMaintenance = 4
}

public enum MilitaryRank
{
    Soldier = 1,              // جندي (مجند)
    Corporal = 2,             // عريف
    Sergeant = 3,             // رقيب
    FirstSergeant = 4,        // رقيب أول
    WarrantOfficer = 5,       // مساعد
    SeniorWarrantOfficer = 6, // مساعد أول
    Cadet = 7,                // طالب
    Lieutenant = 8,           // ملازم
    FirstLieutenant = 9,      // ملازم أول
    Captain = 10,             // نقيب
    Major = 11,               // رائد
    LieutenantColonel = 12,   // مقدم
    Colonel = 13,             // عقيد
    BrigadierGeneral = 14,    // عميد
    MajorGeneral = 15         // لواء
}

