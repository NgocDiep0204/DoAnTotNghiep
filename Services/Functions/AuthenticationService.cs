using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Data;
using api.DTOs.Authentication;
using api.Models;
using api.Responses;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace api.Services.Functions;

public class AuthenticationService : IAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthenticationService(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, ApplicationDbContext context, ITokenService tokenService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _context = context;
        _configuration = configuration;
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
        //if (loginDto is null) return new ServiceResponse.LoginResponse(false, "Request is null", null!);
        // var email = loginDto.Email.Trim();
        var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);
        if (existingUser == null) return new ServiceResponse.LoginResponse(false, "User doesn't exist", null!);


        var currentTokens = await _context.UserTokens.FirstOrDefaultAsync(x => x.UserId == existingUser.Id);
        if (currentTokens != null && currentTokens.Value != null)
            await _tokenService.RevokeTokenAsync(currentTokens.Value, DateTime.UtcNow.AddMinutes(2));

        var isPasswordValid = loginDto.Password != null &&
                              await _userManager.CheckPasswordAsync(existingUser, loginDto.Password);
        if (!isPasswordValid) return new ServiceResponse.LoginResponse(false, "Invalid password", null!);

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Name, existingUser.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var jwtToken = GetToken(authClaims);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        var tokenDescriptor = new IdentityUserToken<string>
        {
            UserId = existingUser.Id,
            LoginProvider = "JWT",
            Name = "AccessToken",
            Value = tokenString
        };

        await _userManager.SetAuthenticationTokenAsync(existingUser, tokenDescriptor.LoginProvider,
            tokenDescriptor.Name, tokenString);

        return new ServiceResponse.LoginResponse(true, "Logined successfully", tokenString);
    }

    public string GenerateRandomOtp()
    {
        Random generator = new();
        var otp = generator.Next(0, 999999).ToString("D6");
        return otp;
    }

    public async Task RemoveExpiredOtps()
    {
        var currentTime = DateTime.UtcNow;
        await _context.OtpStorages
            .Where(o => o.ExpiryTime < currentTime)
            .ExecuteDeleteAsync();
    }

    public JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSiginKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            expires: DateTime.UtcNow.AddMonths(2),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSiginKey, SecurityAlgorithms.HmacSha256)
        );
        return token;
    }
}