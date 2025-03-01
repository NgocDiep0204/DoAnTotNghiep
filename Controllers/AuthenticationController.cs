using System.Security.Claims;
using api.Data;
using api.DTOs.Authentication;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthenticationController(IAuthenticationService authenticationService, ApplicationDbContext context,
        SignInManager<ApplicationUser> signInManager, ITokenService tokenService,
        UserManager<ApplicationUser> userManager)
    {
        _authenticationService = authenticationService;
        _context = context;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authenticationService.RegisterAsync(registerDto);
        if (result.Flag == false) return StatusCode(StatusCodes.Status400BadRequest, result.Message);

        return Ok(new { result.userId });
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _authenticationService.LoginAsync(loginDto);
        if (result.Flag == false) return StatusCode(StatusCodes.Status400BadRequest, result.Message);
        return Ok(new { result.Token });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var currentToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userToken = _context.UserTokens.FirstOrDefault(ut => ut.Value == currentToken);
        if (userToken != null)
        {
            _context.UserTokens.Remove(userToken);
            await _signInManager.SignOutAsync();
            await _context.SaveChangesAsync();
            await _tokenService.RevokeTokenAsync(currentToken, DateTime.UtcNow.AddMinutes(2));
            return StatusCode(StatusCodes.Status200OK, "Logged out successfully from current device");
        }

        return StatusCode(StatusCodes.Status401Unauthorized, "Login Failed");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        if (userName == null)
            return StatusCode(StatusCodes.Status404NotFound,
                new { Status = "Error", StatusMessage = "User not found" });
        var user = _userManager.Users.FirstOrDefault(x => x.UserName == userName);
        if (user == null)
            return StatusCode(StatusCodes.Status404NotFound,
                new { Status = "Error", StatusMessage = "User not found" });

        var isOldPasswordValid = await _userManager.CheckPasswordAsync(user, changePasswordDto.CurrentPassword);
        if (!isOldPasswordValid)
            return StatusCode(StatusCodes.Status400BadRequest,
                new { Status = "Error", StatusMessage = "Old password is incorrect" });

        var result =
            await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword);

        if (!result.Succeeded)
            return StatusCode(StatusCodes.Status400BadRequest, new { Status = "Error", StatusMessage = "failed" });

        return StatusCode(StatusCodes.Status200OK,
            new { Status = "Sucessed", StatusMessage = "Change Password sucessfully" });
    }
}