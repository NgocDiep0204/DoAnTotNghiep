using api.DTOs.Authentication;
using api.Models;
using api.Responses;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace api.Services.Functions;

public class AuthenticationService : IAuthenticationService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthenticationService(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ServiceResponse.RegisterResponse> RegisterAsync(RegisterDto registerDto)
    {
        //if (registerDto is null) return new ServiceResponse.RegisterResponse(false, "registerDto is null", null);

        if (registerDto.Email != null)
        {
            var user = await _userManager.FindByEmailAsync(registerDto.Email);
            if (user != null) return new ServiceResponse.RegisterResponse(false, "user already exists", null);
        }

        var newUser = new ApplicationUser
        {
            Email = registerDto.Email,
            UserName = registerDto.Email,
            FullName = registerDto.FullName,
            Id = Guid.NewGuid().ToString()
        };
        if (registerDto.Role == null || !await _roleManager.RoleExistsAsync(registerDto.Role))
            return new ServiceResponse.RegisterResponse(false, "Role does not exist", null!);

        if (registerDto.Password != null)
        {
            var result = await _userManager.CreateAsync(newUser, registerDto.Password);
            if (!result.Succeeded) return new ServiceResponse.RegisterResponse(false, result.Errors.ToString(), null);
        }

        await _userManager.AddToRoleAsync(newUser, registerDto.Role);
        return new ServiceResponse.RegisterResponse(true, "User created successfully", newUser.Id);
    }

    public async Task<ServiceResponse.LoginResponse> LoginAsync(LoginDto? loginDto)
    {
        if (loginDto is null) return new ServiceResponse.LoginResponse(false, "loginDto is null", null);
        if (loginDto.Email != null)
        {
            var email = loginDto.Email.Trim();
            var existsUser = await _userManager.FindByEmailAsync(email);
            if (existsUser == null) return new ServiceResponse.LoginResponse(false, "user does not exist", null!);
            var curruntToken = await _userManager.GeneratePasswordResetTokenAsync(existsUser);
        }

        throw new NotImplementedException();
    }
}