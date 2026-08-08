using Contracts.DTOs.User;
using Contracts.interfaces.Repository;
using Contracts.Responses;
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
        var allowedPages = await _repositoryWrapper.UserPagePermissions.GetAllowedPagesForUserAsync(user.Id);

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            PersonName = user.PersonName,
            UserGroupId = user.UserGroupId,
            UserGroupName = user.UserGroup?.Name
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

        var tempUser = new User { Username = registerDto.Username };
        string hashedPassword = PasswordHasherHelper.HashPassword(tempUser, registerDto.Password);

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = hashedPassword,
            Role = registerDto.Role,
            PersonName = registerDto.PersonName,
            UserGroupId = registerDto.UserGroupId
        };

        await _repositoryWrapper.Users.Create(new UserCreateDto
        {
            Username = user.Username,
            Email = user.Email,
            Password = user.PasswordHash,
            Role = user.Role,
            PersonName = user.PersonName,
            UserGroupId = user.UserGroupId
        });

        return Ok(new SingleObjectResponseModel
        {
            IsDone = true,
            ReturnMessage = "User registered successfully"
        });
    }
}
