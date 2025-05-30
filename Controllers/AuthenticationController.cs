using System.Security.Claims;
using api.Data;
using api.DTOs.Authentication;
using api.Models;
using api.Services.Interfaces;
using api.Services.MailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ApplicationDbContext _context;
    private readonly IMailService _mailService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthenticationController(IAuthenticationService authenticationService, ApplicationDbContext context,
        SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IMailService mailService,
        UserManager<ApplicationUser> userManager)
    {
        _authenticationService = authenticationService;
        _context = context;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _userManager = userManager;
        _mailService = mailService;
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
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authenticationService.LoginAsync(loginDto);

        if (!result.Flag)
        {
            // Phân loại lỗi cụ thể
            return result.Message switch
            {
                "User doesn't exist" => NotFound(result.Message),
                "Invalid password" => StatusCode(StatusCodes.Status409Conflict,result.Message),
                "Account is inactive" => StatusCode(StatusCodes.Status403Forbidden, result.Message),
                _ => BadRequest(result.Message)
            };
        }

        return Ok(new { result.Token });
    }


    [HttpPost]
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
        }
        return Ok("Logged out successfully");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        if (userName == null)
            return StatusCode(StatusCodes.Status404NotFound, "User not found");
        var user = _userManager.Users.FirstOrDefault(x => x.UserName == userName);
        if (user == null)
            return StatusCode(StatusCodes.Status404NotFound, "User not found");

        var isOldPasswordValid = await _userManager.CheckPasswordAsync(user, changePasswordDto.CurrentPassword);
        if (!isOldPasswordValid)
            return StatusCode(StatusCodes.Status400BadRequest, "Old password is incorrect");

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword,
            changePasswordDto.NewPassword);

        if (!result.Succeeded)
            return StatusCode(StatusCodes.Status400BadRequest, "failed");

        return StatusCode(StatusCodes.Status200OK, "Change Password sucessfully");
    }

    [HttpPost]
    public async Task<IActionResult> SendOtp(string email)
    {
        var _email = email.Trim();
        if (string.IsNullOrEmpty(_email))
            return StatusCode(StatusCodes.Status400BadRequest, new { Status = "Error", StatusMessage = "empty email" });
        var existingUser = await _userManager.FindByEmailAsync(_email);
        if (existingUser != null)
        {
            var existingOtp = await _context.OtpStorages.FirstOrDefaultAsync(o => o.UserId == existingUser.Id);
            if (existingOtp != null) _context.OtpStorages.Remove(existingOtp);
        }
        else
        {
            return StatusCode(StatusCodes.Status404NotFound, "User not found");
        }

        var otp = _authenticationService.GenerateRandomOtp();
        var otpRecord = new OtpStorage
        {
            Id = Guid.NewGuid().ToString(),
            UserId = existingUser.Id,
            Otp = otp,
            ExpiryTime = DateTime.UtcNow.AddMinutes(15)
        };

        _context.OtpStorages.Add(otpRecord);
        await _context.SaveChangesAsync();

        var body = $@"Your OTP is: {otp}. Note: this OTP will be out of time after 2 minutes";

        var message = new MailMessages(new[] { _email }, "OTP Request", body, false);
        _mailService.SendEmail(message);
        return Ok(new { otp });
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOtp(string userEmail, string otp)
    {
        var email = userEmail.Trim();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return StatusCode(StatusCodes.Status404NotFound,
                new { Status = "Error", StatusMessage = "User not found, email does not exist" });
        var exignOtp = await _context.OtpStorages.FirstOrDefaultAsync(o => o.Otp == otp && o.UserId == user.Id);
        if (exignOtp == null)
            return StatusCode(StatusCodes.Status400BadRequest, new { Status = "Error", StatusMessage = "otp invalid" });
        if (exignOtp.ExpiryTime <= DateTime.UtcNow)
            return StatusCode(StatusCodes.Status410Gone, new { Status = "Error", StatusMessage = "OTP is expired" });
        return StatusCode(StatusCodes.Status200OK, new { Status = "Success", StatusMessage = "OTP is valid" });
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var email = resetPasswordDto.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return StatusCode(StatusCodes.Status404NotFound,
                new { Status = "Error", StatusMessage = "user not found" });

        await _authenticationService.RemoveExpiredOtps();

        var otpRecord =
            await _context.OtpStorages.FirstOrDefaultAsync(o => o.UserId == user.Id && o.Otp == resetPasswordDto.Otp);
        if (otpRecord == null)
            return StatusCode(StatusCodes.Status404NotFound, new { Status = "Error", StatusMessage = "OTP not found" });
        if (otpRecord.ExpiryTime <= DateTime.UtcNow) return StatusCode(StatusCodes.Status410Gone, new { Status = "Error", StatusMessage = "otp expired" });
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword);
        if (result.Succeeded)
        {
            _context.OtpStorages.Remove(otpRecord);
            await _context.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, "Password reset successfully");
        }

        return StatusCode(StatusCodes.Status400BadRequest, result.Errors);
    }
    
   
}