using System.Net;
using System.Security.Claims;
using api.DTOs;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ApplicationUserController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationUserController(UserManager<ApplicationUser> userManager, IImageService imageService,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _imageService = imageService;
        _roleManager = roleManager;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUserProfile()
    {
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;

        if (userName == null)
            return StatusCode(StatusCodes.Status404NotFound, new
            {
                Status = "Error",
                StatusMessage = "User not found"
            });

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            return StatusCode(StatusCodes.Status404NotFound, new
            {
                Status = "Error",
                StatusMessage = "User not found"
            });

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.UserName,
            user.ImageUrl,
            user.PhoneNumber,
            Role = role
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUserByRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return BadRequest("Role is required.");

        var users = await _userManager.GetUsersInRoleAsync(role);

        // Tùy ý trả về thông tin cần thiết (ẩn bớt sensitive data)
        var result = users.Select(u => new
        {
            u.Id,
            u.UserName,
            u.Email,
            u.FullName,
            u.Status,
            Roles = role
        });

        return Ok(result);
    }


    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateUserProfile([FromForm] ApplicationUserDto user)
    {
        if (ModelState.IsValid)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userName == null)
                return StatusCode(StatusCodes.Status404NotFound, "User not found");
            var userToUpdate = _userManager.Users.FirstOrDefault(x => x.UserName == userName);
            if (userToUpdate == null)
                return StatusCode(StatusCodes.Status404NotFound, "User not found");

            string? imgPath = null;
            if (user.FormFile != null)
            {
                var uploadResult = await _imageService.AddImageAsync(user.FormFile);
                if (uploadResult.StatusCode == HttpStatusCode.OK)
                    imgPath = uploadResult.SecureUrl.AbsoluteUri;
                else
                    return StatusCode((int)uploadResult.StatusCode, "Image upload failed.");
            }

            userToUpdate.Gender = user.Gender;
            userToUpdate.FullName = user.FullName;
            userToUpdate.ImageUrl = imgPath;
            var result = await _userManager.UpdateAsync(userToUpdate);
            if (result.Succeeded) return Ok(new { Status = "Success", StatusMessage = "User updated successfully" });
            return StatusCode(StatusCodes.Status500InternalServerError, "User update failed");
        }

        return BadRequest(ModelState);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUserById(string id, string email, string fullname, Status status)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return StatusCode(StatusCodes.Status404NotFound, "User not found");
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null && existingUser.Id != user.Id)
            return Conflict("Email đã được sử dụng bởi người dùng khác.");
        user.FullName = fullname;
        user.Email = email;
        user.UserName = email;
        user.Status = status;
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded) return Ok(new { Status = "Success", StatusMessage = "User updated successfully" });

        // Đọc lỗi cụ thể để debug
        var errors = result.Errors.Select(e => e.Description).ToList();
        return StatusCode(StatusCodes.Status400BadRequest, new { Status = "Failed", Errors = errors });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateUserRole(string id, string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded) return BadRequest("Không thể gỡ role cũ.");

        // Tạo role nếu chưa tồn tại
        if (!await _roleManager.RoleExistsAsync(role)) await _roleManager.CreateAsync(new IdentityRole(role));

        var addResult = await _userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded) return BadRequest("Không thể gán role mới.");

        return Ok(new { message = "Cập nhật role thành công." });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUserStatus(string id, Status status)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return StatusCode(StatusCodes.Status404NotFound, "User not found");
        user.Status = status;
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded) return Ok(new { Status = "Success", StatusMessage = "User updated successfully" });
        return StatusCode(StatusCodes.Status500InternalServerError, "User update failed");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded) return Ok(new { Status = "Success", StatusMessage = "User deleted successfully" });
        return StatusCode(StatusCodes.Status500InternalServerError, "User delete failed");
    }
}