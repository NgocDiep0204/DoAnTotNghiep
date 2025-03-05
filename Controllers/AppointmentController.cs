using System.Security.Claims;
using api.Data;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AppointmentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IDentalService _dentalService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AppointmentController(ApplicationDbContext context, IDentalService dentalService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _dentalService = dentalService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointmentsByUserId(string userId)
    {
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        if (userName == null) return StatusCode(StatusCodes.Status401Unauthorized, "User not found");
        var user = await _userManager.FindByNameAsync(userName);
        if (user != null)
        {
            var appoinmentList = _dentalService.GetServicesAsync(userId);
            return Ok(appoinmentList);
        }

        return StatusCode(StatusCodes.Status401Unauthorized, "User not found");
    }
}