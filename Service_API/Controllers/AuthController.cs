using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Contracts.DTOs.User;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Enums;
using Entities.Models.Tables;
using LoggerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_API.BaseControllers;
using Service_API.Helpers;

namespace Service_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController<User, UserDto, UserCreateDto, UserUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILoggerManager _logger;

    public AuthController(IRepositoryWrapper repositoryWrapper, IJwtTokenGenerator jwtTokenGenerator, ILoggerManager logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.Users;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _repositoryWrapper.Users.GetByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                return Unauthorized(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = "اسم المستخدم أو كلمة المرور غير صحيحة"
                });
            }

            // Verify Branch status if user belongs to a branch
            if (user.Branch != null && (!user.Branch.IsActive || user.Branch.IsDeleted))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = "حساب الفرع الخاص بك معلق أو غير نشط حالياً. يرجى مراجعة إدارة المنصة."
                });
            }

            bool isPasswordValid = PasswordHasherHelper.VerifyPassword(user, user.PasswordHash, loginDto.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = "اسم المستخدم أو كلمة المرور غير صحيحة"
                });
            }

            var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);
            var allowedPages = await _repositoryWrapper.UserPagePermissions.GetAllowedPagesForUserAsync(user.MilitaryNumber);

            var userDto = new UserDto
            {
                MilitaryNumber = user.MilitaryNumber,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                PersonName = user.PersonName,
                UserGroupId = user.UserGroupId,
                UserGroupName = user.UserGroup?.Name,
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name,
                MustChangePassword = user.MustChangePassword
            };

            var authResponse = new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = userDto,
                AllowedPages = allowedPages
            };

            return Ok(new SingleObjectResponseModel<AuthResponseDto>
            {
                IsDone = true,
                ReturnMessage = "تم تسجيل الدخول بنجاح",
                SingleObject = authResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during login: {ex}");
            return StatusCode(StatusCodes.Status500InternalServerError, new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "حدث خطأ غير متوقع أثناء تسجيل الدخول. يرجى المحاولة مرة أخرى."
            });
        }
    }

    [HttpGet("bootstrap")]
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetBootstrap()
    {
        try
        {
            var militaryNumberClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(militaryNumberClaim, out var militaryNumber))
                return Unauthorized();

            var user = await _repositoryWrapper.Users.GetByIdWithGroupAsync(militaryNumber);
            if (user == null)
            {
                return NotFound(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = "المستخدم غير موجود"
                });
            }

            var allowedPages = await _repositoryWrapper.UserPagePermissions.GetAllowedPagesForUserAsync(user.MilitaryNumber);

            var userDto = new UserDto
            {
                MilitaryNumber = user.MilitaryNumber,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                PersonName = user.PersonName,
                UserGroupId = user.UserGroupId,
                UserGroupName = user.UserGroup?.Name,
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name,
                MustChangePassword = user.MustChangePassword
            };

            return Ok(new SingleObjectResponseModel<BootstrapResponseDto>
            {
                IsDone = true,
                ReturnMessage = "بيانات الجلسة والصلاحيات تم تحميلها بنجاح",
                SingleObject = new BootstrapResponseDto
                {
                    User = userDto,
                    AllowedPages = allowedPages
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in GetBootstrap: {ex}");
            return StatusCode(StatusCodes.Status500InternalServerError, new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "حدث خطأ أثناء تحميل بيانات المستخدم والصلاحيات."
            });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var militaryNumberClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(militaryNumberClaim, out var militaryNumber))
                return Unauthorized();

            var user = await _repositoryWrapper.Users.GetByIdWithGroupAsync(militaryNumber);
            if (user == null)
                return NotFound();

            bool isCurrentValid = PasswordHasherHelper.VerifyPassword(user, user.PasswordHash, dto.CurrentPassword);
            if (!isCurrentValid)
            {
                return BadRequest(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = "كلمة المرور الحالية غير صحيحة"
                });
            }

            user.PasswordHash = PasswordHasherHelper.HashPassword(user, dto.NewPassword);
            user.MustChangePassword = false;
            await _repositoryWrapper.SaveAsync();

            return Ok(new SingleObjectResponseModel
            {
                IsDone = true,
                ReturnMessage = "تم تغيير كلمة المرور بنجاح"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in ChangePassword: {ex}");
            return StatusCode(StatusCodes.Status500InternalServerError, new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "تعذر تغيير كلمة المرور حالياً. يرجى المحاولة لاحقاً."
            });
        }
    }

    [HttpGet]
    [Authorize]
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        try
        {
            int? branchId = null;
            if (HttpContext.Request.Query.TryGetValue("branchId", out var branchIdVal) && int.TryParse(branchIdVal, out var parsedBranchId))
            {
                branchId = parsedBranchId;
            }

            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            bool isSuperAdmin = roleClaim == "SuperAdmin" || User.IsInRole("SuperAdmin");
            var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
            int? userBranchId = int.TryParse(branchClaim, out var bId) && bId > 0 ? bId : null;

            var users = await _repositoryWrapper.Users.GetUsersFilteredAsync(branchId, isSuperAdmin, userBranchId);

            return Ok(new ListOfObjectsResponseModel<UserDto>
            {
                IsDone = true,
                ErrorCode = Contracts.enums.ErrorCatalog.noError,
                ReturnMessage = "Users loaded successfully",
                Objects = users,
                TotalCount = users.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in GetAll Users: {ex}");
            return StatusCode(StatusCodes.Status500InternalServerError, new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "حدث خطأ أثناء جلب قائمة المستخدمين."
            });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] UserCreateDto registerDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _repositoryWrapper.Users.GetByUsernameAsync(registerDto.Username.Trim());
            if (existingUser != null)
            {
                return BadRequest(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = "اسم المستخدم مسجل مسبقاً، يرجى اختيار اسم مستخدم آخر."
                });
            }

            if (registerDto.MilitaryNumber > 0)
            {
                var existingMilitary = await _repositoryWrapper.Users.GetByIdWithGroupAsync(registerDto.MilitaryNumber);
                if (existingMilitary != null)
                {
                    return BadRequest(new SingleObjectResponseModel
                    {
                        IsDone = false,
                        ReturnMessage = "الرقم العسكري مسجل مسبقاً لمستخدم آخر."
                    });
                }
            }

            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            bool isSuperAdmin = roleClaim == "SuperAdmin" || User.IsInRole("SuperAdmin");
            var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
            int? userBranchId = int.TryParse(branchClaim, out var bId) && bId > 0 ? bId : null;

            int targetBranchId;
            if (isSuperAdmin)
            {
                targetBranchId = (registerDto.BranchId.HasValue && registerDto.BranchId.Value > 0) ? registerDto.BranchId.Value : 1;
            }
            else
            {
                targetBranchId = userBranchId ?? (registerDto.BranchId.HasValue && registerDto.BranchId.Value > 0 ? registerDto.BranchId.Value : 1);
                if (registerDto.Role == UserRole.SuperAdmin)
                {
                    registerDto.Role = UserRole.Employee;
                }
            }

            // Enforce MaxUsers quota configured by SuperAdmin
            var branch = await _repositoryWrapper.Branches.GetByIdAsync(targetBranchId);
            if (branch != null && branch.MaxUsers > 0 && !isSuperAdmin)
            {
                var currentUsersCount = await _repositoryWrapper.Branches.GetActiveUserCountAsync(targetBranchId);
                if (currentUsersCount >= branch.MaxUsers)
                {
                    return BadRequest(new SingleObjectResponseModel
                    {
                        ErrorCode = Contracts.enums.ErrorCatalog.missingValues,
                        IsDone = false,
                        ReturnMessage = $"لقد تم استهلاك الحد الأقصى للمستخدمين المسموح به لهذا الفرع ({branch.MaxUsers} مستخدم). تم تعيين هذا الحد بواسطة مدير المنصة (SuperAdmin)."
                    });
                }
            }

            int targetMilitaryNumber = registerDto.MilitaryNumber;
            if (targetMilitaryNumber <= 0)
            {
                targetMilitaryNumber = (targetBranchId * 10000) + 1;
                while (await _repositoryWrapper.Users.GetByIdWithGroupAsync(targetMilitaryNumber) != null)
                {
                    targetMilitaryNumber++;
                }
            }

            int? validUserGroupId = (registerDto.UserGroupId.HasValue && registerDto.UserGroupId.Value > 0)
                ? registerDto.UserGroupId.Value
                : null;

            string rawPassword = string.IsNullOrWhiteSpace(registerDto.Password) ? "P@ssw0rd" : registerDto.Password;
            var tempUser = new User { Username = registerDto.Username, MilitaryNumber = targetMilitaryNumber };
            string hashedPassword = PasswordHasherHelper.HashPassword(tempUser, rawPassword);

            var user = new User
            {
                MilitaryNumber = targetMilitaryNumber,
                Username = registerDto.Username.Trim(),
                Email = registerDto.Email.Trim(),
                PasswordHash = hashedPassword,
                Role = registerDto.Role != 0 ? registerDto.Role : UserRole.Employee,
                PersonName = registerDto.PersonName?.Trim() ?? registerDto.Username.Trim(),
                UserGroupId = validUserGroupId,
                BranchId = targetBranchId,
                MustChangePassword = true
            };

            await _repositoryWrapper.Users.CreateDirectAsync(user);
            await _repositoryWrapper.SaveAsync();

            var createdUserDto = new UserDto
            {
                MilitaryNumber = user.MilitaryNumber,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                PersonName = user.PersonName,
                UserGroupId = user.UserGroupId,
                UserGroupName = user.UserGroup?.Name,
                BranchId = user.BranchId,
                BranchName = branch?.Name,
                MustChangePassword = true
            };

            return Ok(new SingleObjectResponseModel<UserDto>
            {
                IsDone = true,
                ReturnMessage = "تم إنشاء حساب المستخدم بنجاح",
                SingleObject = createdUserDto
            });
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError($"DbUpdateException in Register: {dbEx}");
            var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
            string userMsg = "تعذر حفظ بيانات المستخدم في قاعدة البيانات.";
            if (innerMsg.Contains("IX_Users_Username") || innerMsg.Contains("Username"))
                userMsg = "اسم المستخدم موجود مسبقاً.";
            else if (innerMsg.Contains("IX_Users_Email") || innerMsg.Contains("Email"))
                userMsg = "البريد الإلكتروني مسجل مسبقاً.";
            else if (innerMsg.Contains("PK_Users") || innerMsg.Contains("MilitaryNumber"))
                userMsg = "الرقم العسكري مسجل مسبقاً.";

            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = userMsg
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unhandled exception in Register: {ex}");
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "حدث خطأ أثناء حفظ بيانات المستخدم. يرجى التأكد من صحة البيانات والفرع."
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public override async Task<IActionResult> Update([FromRoute] string id, [FromBody] UserUpdateDto updateDto)
    {
        try
        {
            if (!int.TryParse(id, out var militaryNumber))
                return BadRequest("Invalid user ID");

            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            bool isSuperAdmin = roleClaim == "SuperAdmin" || User.IsInRole("SuperAdmin");
            var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
            int? userBranchId = int.TryParse(branchClaim, out var bId) && bId > 0 ? bId : null;

            var existingUser = await _repositoryWrapper.Users.GetByIdWithGroupAsync(militaryNumber);
            if (existingUser == null)
                return NotFound(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "المستخدم غير موجود" });

            if (!isSuperAdmin && existingUser.BranchId != userBranchId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = "لا تملك صلاحية تعديل بيانات مستخدمين تابعين لفروع أخرى"
                });
            }

            existingUser.Username = updateDto.Username.Trim();
            existingUser.Email = updateDto.Email.Trim();
            existingUser.PersonName = updateDto.PersonName?.Trim();
            existingUser.Role = (!isSuperAdmin && updateDto.Role == UserRole.SuperAdmin) ? existingUser.Role : (updateDto.Role != 0 ? updateDto.Role : existingUser.Role);
            existingUser.UserGroupId = (updateDto.UserGroupId.HasValue && updateDto.UserGroupId.Value > 0) ? updateDto.UserGroupId.Value : null;

            if (isSuperAdmin && updateDto.BranchId.HasValue && updateDto.BranchId.Value > 0)
            {
                existingUser.BranchId = updateDto.BranchId.Value;
            }
            else if (!isSuperAdmin && userBranchId.HasValue)
            {
                existingUser.BranchId = userBranchId.Value;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Password))
            {
                existingUser.PasswordHash = PasswordHasherHelper.HashPassword(existingUser, updateDto.Password);
            }

            await _repositoryWrapper.SaveAsync();

            var updatedUser = await _repositoryWrapper.Users.GetByIdWithGroupAsync(militaryNumber);
            var updatedUserDto = new UserDto
            {
                MilitaryNumber = updatedUser!.MilitaryNumber,
                Username = updatedUser.Username,
                Email = updatedUser.Email,
                Role = updatedUser.Role,
                PersonName = updatedUser.PersonName,
                UserGroupId = updatedUser.UserGroupId,
                UserGroupName = updatedUser.UserGroup?.Name,
                BranchId = updatedUser.BranchId,
                BranchName = updatedUser.Branch?.Name,
                MustChangePassword = updatedUser.MustChangePassword
            };

            return Ok(new SingleObjectResponseModel<UserDto>
            {
                IsDone = true,
                ReturnMessage = "تم تحديث بيانات المستخدم بنجاح",
                SingleObject = updatedUserDto
            });
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError($"DbUpdateException in Update: {dbEx}");
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "تعذر تحديث البيانات بقاعدة البيانات، يرجى التأكد من عدم تكرار اسم المستخدم أو البريد."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unhandled exception in Update: {ex}");
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "حدث خطأ أثناء تعديل بيانات المستخدم."
            });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public override async Task<IActionResult> Delete([FromRoute] string id)
    {
        try
        {
            if (!int.TryParse(id, out var militaryNumber))
                return BadRequest("Invalid user ID");

            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            bool isSuperAdmin = roleClaim == "SuperAdmin" || User.IsInRole("SuperAdmin");
            var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
            int? userBranchId = int.TryParse(branchClaim, out var bId) && bId > 0 ? bId : null;

            var existingUser = await _repositoryWrapper.Users.GetByIdWithGroupAsync(militaryNumber);
            if (existingUser == null)
                return NotFound(new SingleObjectResponseModel { IsDone = false, ReturnMessage = "المستخدم غير موجود" });

            if (existingUser.Role == UserRole.SuperAdmin)
            {
                return BadRequest(new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = "لا يمكن حذف حساب مدير المنصة العام (SuperAdmin)"
                });
            }

            if (!isSuperAdmin && existingUser.BranchId != userBranchId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleObjectResponseModel
                {
                    IsDone = false,
                    ReturnMessage = "لا تملك صلاحية حذف مستخدمين تابعين لفروع أخرى"
                });
            }

            existingUser.IsDeleted = true;
            existingUser.DeleteDate = DateTime.UtcNow;
            await _repositoryWrapper.SaveAsync();

            return Ok(new SingleObjectResponseModel
            {
                IsDone = true,
                ReturnMessage = "تم حذف المستخدم بنجاح"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in Delete user: {ex}");
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "حدث خطأ أثناء محاولة حذف المستخدم."
            });
        }
    }
}
