using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AppointmentDetailController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AppointmentDetailController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointmentDetail(AppointmentDetailDto appointmentDetailDto)
    {
        var newAppoinmetDetail = new AppointmentDetails
        {
            AppointmentId = appointmentDetailDto.AppointmentId,
            ServiceId = appointmentDetailDto.ServiceId
        };
        _context.AppointmentDetails.Add(newAppoinmetDetail);
        return await _context.SaveChangesAsync() > 0
            ? StatusCode(StatusCodes.Status200OK, "Success")
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAppointmentDetail(AppointmentDetailDto appointmentDetailDto)
    {
        var appointmentDetail = _context.AppointmentDetails.Where(a=> a.AppointmentId == appointmentDetailDto.AppointmentId
                                                                                                            && a.ServiceId == appointmentDetailDto.ServiceId);
        _context.Remove(appointmentDetail);
        return await _context.SaveChangesAsync() > 0
            ? StatusCode(StatusCodes.Status200OK, "Success")
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }
}