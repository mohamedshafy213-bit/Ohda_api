using System.Security.Claims;
using Contracts.DTOs.User;
using Contracts.interfaces.Repository;
using Contracts.Responses;
using Entities.Models.Enums;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_API.BaseControllers;
using Service_API.Helpers;

namespace Service_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController<User, UserDto, UserCreateDto, UserUpdateDto>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthController(IRepositoryWrapper repositoryWrapper, IJwtTokenGenerator jwtTokenGenerator)
    {
        _repositoryWrapper = repositoryWrapper;
        _repository = repositoryWrapper.Users;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _repositoryWrapper.Users.GetByUsernameAsync(loginDto.Username);
        if (user == null)
        {
            return Unauthorized(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Invalid username or password"
            });
        }

        // Verify Branch status if user belongs to a branch
        if (user.Branch != null && (!user.Branch.IsActive || user.Branch.IsDeleted))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Your branch account is currently suspended or inactive. Please contact system administration."
            });
        }

        bool isPasswordValid = PasswordHasherHelper.VerifyPassword(user, user.PasswordHash, loginDto.Password);
        if (!isPasswordValid)
        {
            return Unauthorized(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Invalid username or password"
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
            ReturnMessage = "Login successful",
            SingleObject = authResponse
        });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
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
                ReturnMessage = "Current password is not correct"
            });
        }

        user.PasswordHash = PasswordHasherHelper.HashPassword(user, dto.NewPassword);
        user.MustChangePassword = false;
        await _repositoryWrapper.SaveAsync();

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = "Password changed successfully"
        });
    }

    [HttpGet]
    [Authorize]
    public override async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
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

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] UserCreateDto registerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingUser = await _repositoryWrapper.Users.GetByUsernameAsync(registerDto.Username);
        if (existingUser != null)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Username already exists"
            });
        }

        var existingMilitary = await _repositoryWrapper.Users.GetByIdWithGroupAsync(registerDto.MilitaryNumber);
        if (existingMilitary != null)
        {
            return BadRequest(new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "Military number already exists"
            });
        }

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        bool isSuperAdmin = roleClaim == "SuperAdmin" || User.IsInRole("SuperAdmin");
        var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
        int? userBranchId = int.TryParse(branchClaim, out var bId) && bId > 0 ? bId : null;

        int targetBranchId;
        if (isSuperAdmin)
        {
            targetBranchId = registerDto.BranchId ?? 1;
        }
        else
        {
            // Branch Admin is strictly locked to their own branch
            targetBranchId = userBranchId ?? registerDto.BranchId ?? 1;
            // Prevent Branch Admin from creating SuperAdmins
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

        var tempUser = new User { Username = registerDto.Username, MilitaryNumber = registerDto.MilitaryNumber };
        string hashedPassword = PasswordHasherHelper.HashPassword(tempUser, registerDto.Password);

        var user = new User
        {
            MilitaryNumber = registerDto.MilitaryNumber,
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = hashedPassword,
            Role = registerDto.Role,
            PersonName = registerDto.PersonName,
            UserGroupId = registerDto.UserGroupId,
            BranchId = targetBranchId
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
            BranchName = branch?.Name
        };

        return Ok(new SingleObjectResponseModel<UserDto>
        {
            IsDone = true,
            ReturnMessage = "User registered successfully",
            SingleObject = createdUserDto
        });
    }

    [HttpPut("{id}")]
    [Authorize]
    public override async Task<IActionResult> Update([FromRoute] string id, [FromBody] UserUpdateDto updateDto)
    {
        if (!int.TryParse(id, out var militaryNumber))
            return BadRequest("Invalid user ID");

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        bool isSuperAdmin = roleClaim == "SuperAdmin" || User.IsInRole("SuperAdmin");
        var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
        int? userBranchId = int.TryParse(branchClaim, out var bId) && bId > 0 ? bId : null;

        var existingUser = await _repositoryWrapper.Users.GetByIdWithGroupAsync(militaryNumber);
        if (existingUser == null)
            return NotFound();

        if (!isSuperAdmin && existingUser.BranchId != userBranchId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new SingleObjectResponseModel
            {
                IsDone = false,
                ReturnMessage = "لا تملك صلاحية تعديل بيانات مستخدمين تابعين لفروع أخرى"
            });
        }

        existingUser.Username = updateDto.Username;
        existingUser.Email = updateDto.Email;
        existingUser.PersonName = updateDto.PersonName;
        existingUser.Role = (!isSuperAdmin && updateDto.Role == UserRole.SuperAdmin) ? existingUser.Role : updateDto.Role;
        existingUser.UserGroupId = updateDto.UserGroupId;

        if (isSuperAdmin && updateDto.BranchId.HasValue)
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

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = "User updated successfully"
        });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public override async Task<IActionResult> Delete([FromRoute] string id)
    {
        if (!int.TryParse(id, out var militaryNumber))
            return BadRequest("Invalid user ID");

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        bool isSuperAdmin = roleClaim == "SuperAdmin" || User.IsInRole("SuperAdmin");
        var branchClaim = User.FindFirst("branch_id")?.Value ?? User.FindFirst("BranchId")?.Value;
        int? userBranchId = int.TryParse(branchClaim, out var bId) && bId > 0 ? bId : null;

        var existingUser = await _repositoryWrapper.Users.GetByIdWithGroupAsync(militaryNumber);
        if (existingUser == null)
            return NotFound();

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
            ReturnMessage = "User deleted successfully"
        });
    }
}
