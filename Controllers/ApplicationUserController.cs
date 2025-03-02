using System.Security.Claims;
using api.DTOs;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApplicationUserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IImageService _imageService;

    public ApplicationUserController(UserManager<ApplicationUser> userManager, IImageService imageService)
    {
        _userManager = userManager;
        _imageService = imageService;
    }

    [Authorize]
    [HttpGet]
    public IActionResult GetUserProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        if (userName == null)
            return StatusCode(StatusCodes.Status404NotFound,
                new { Status = "Error", StatusMessage = "User not found" });
        var user = _userManager.Users.FirstOrDefault(x => x.UserName == userName);
        return Ok(user);
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
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    imgPath = uploadResult.SecureUrl.AbsoluteUri;
                }
                else
                {
                    return StatusCode((int)uploadResult.StatusCode, "Image upload failed.");
                }
            
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
}