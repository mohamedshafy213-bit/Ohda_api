using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class EgyptianNationalIdAttribute : ValidationAttribute
{
    // بناءً على كلام الأحوال المدنية رسميًا
    private const string Pattern = @"^(2|3)\d{6}(0[1-9]|[12]\d|3[01]|88)(?:[0-9]{4})([13579]|0?[2468])\d$";

    private static readonly HashSet<string> ValidGovernorates = new()
    {
        "01","02","03","04","11","12","13","14","15","16",
        "17","18","19","21","22","23","24","25","26","27",
        "28","29","31","32","33","34","35","88"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {

        if (value == null)
        {
        return ValidationResult.Success;
            
        }


        if (value is not string id || string.IsNullOrWhiteSpace(id))
            return new ValidationResult("الرقم القومي مطلوب.");

        if (id.Length != 14 || !id.All(char.IsDigit))
            return new ValidationResult("الرقم القومي يجب أن يتكون من 14 رقمًا بالضبط.");

        // 1. التحقق من القرن (2 أو 3 فقط)
        if (id[0] != '2' && id[0] != '3')
            return new ValidationResult("الرقم القومي يجب أن يبدأ بـ 2 أو 3.");

        // 2. التحقق من تاريخ الميلاد (الأرقام من 2 إلى 7)
        int birthYear = int.Parse(id.Substring(1, 2));
        int birthMonth = int.Parse(id.Substring(3, 2));
        int birthDay = int.Parse(id.Substring(5, 2));

        int fullYear = id[0] == '2' ? 1900 + birthYear : 2000 + birthYear;

        if (!IsValidDate(fullYear, birthMonth, birthDay))
            return new ValidationResult("تاريخ الميلاد في الرقم القومي غير صحيح.");

        // 3. كود المحافظة (الأرقام 8 و9)
        string govCode = id.Substring(7, 2);
        if (!ValidGovernorates.Contains(govCode))
            return new ValidationResult("كود المحافظة غير صحيح.");

        // 4. الرقم الثالث عشر (الموضع 13) = الجنس
        char genderDigit = id[12]; // الفهرس 12 = الرقم الثالث عشر
        if (!char.IsDigit(genderDigit) || 
            (genderDigit != '1' && genderDigit != '3' && genderDigit != '5' && 
             genderDigit != '7' && genderDigit != '9' && 
             genderDigit != '0' && genderDigit != '2' && genderDigit != '4' && 
             genderDigit != '6' && genderDigit != '8'))
            return new ValidationResult("رقم تحديد الجنس في الرقم القومي غير صحيح.");

        // 5. الرقم الأخير (رقم التحقق) من 1 إلى 9 فقط (حسب الأحوال المدنية)
        char checkDigit = id[13];
        if (checkDigit < '1' || checkDigit > '9')
            return new ValidationResult("رقم التحقق في الرقم القومي غير صحيح.");

        return ValidationResult.Success;
    }

    private static bool IsValidDate(int year, int month, int day)
    {
        try
        {
            new DateTime(year, month, day);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Public static method to validate NID without ValidationContext
    public static string? ValidateNationalId(string? id)
    {
        if (id == null)
            return null; // Null is considered valid (optional field)

        if (string.IsNullOrWhiteSpace(id))
            return "NID is required";

        if (id.Length != 14 || !id.All(char.IsDigit))
            return "NID must be exactly 14 digits";

        // 1. Check century (must start with 2 or 3)
        if (id[0] != '2' && id[0] != '3')
            return "NID must start with 2 or 3 (invalid century)";

        // 2. Validate birth date (positions 1-6)
        int birthYear = int.Parse(id.Substring(1, 2));
        int birthMonth = int.Parse(id.Substring(3, 2));
        int birthDay = int.Parse(id.Substring(5, 2));

        int fullYear = id[0] == '2' ? 1900 + birthYear : 2000 + birthYear;

        if (!IsValidDate(fullYear, birthMonth, birthDay))
            return "Invalid birth date in NID";

        // 3. Governorate code (positions 7-8)
        string govCode = id.Substring(7, 2);
        if (!ValidGovernorates.Contains(govCode))
            return "Invalid governorate code in NID";

        // 4. Gender digit (position 12, index 12)
        char genderDigit = id[12];
        if (!char.IsDigit(genderDigit) || 
            (genderDigit != '1' && genderDigit != '3' && genderDigit != '5' && 
             genderDigit != '7' && genderDigit != '9' && 
             genderDigit != '0' && genderDigit != '2' && genderDigit != '4' && 
             genderDigit != '6' && genderDigit != '8'))
            return "Invalid gender digit in NID";

        // 5. Check digit (position 13, index 13) - must be 1-9
        char checkDigit = id[13];
        if (checkDigit < '1' || checkDigit > '9')
            return "Invalid check digit in NID (must be 1-9)";

        return null; // Valid
    }
}