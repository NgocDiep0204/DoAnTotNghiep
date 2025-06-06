using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            ServiceId = appointmentDetailDto.ServiceId,
            Quantity = appointmentDetailDto.Quantity,
        };
        _context.AppointmentDetails.Add(newAppoinmetDetail);
        return await _context.SaveChangesAsync() > 0
            ? StatusCode(StatusCodes.Status200OK, "Success")
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAppointmentDetail(AppointmentDetailDto appointmentDetailDto)
    {
        var existAppointmentDetail = await _context.AppointmentDetails
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentDetailDto.AppointmentId
                                      && a.ServiceId == appointmentDetailDto.ServiceId);
        existAppointmentDetail.Quantity = appointmentDetailDto.Quantity;
        _context.AppointmentDetails.Update(existAppointmentDetail);
        return await _context.SaveChangesAsync() > 0
            ? StatusCode(StatusCodes.Status200OK, "Success")
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAppointmentDetail(AppointmentDetailDto appointmentDetailDto)
    {
        // Lấy entity cụ thể đầu tiên hoặc null nếu không tìm thấy
        var appointmentDetail = await _context.AppointmentDetails
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentDetailDto.AppointmentId
                                      && a.ServiceId == appointmentDetailDto.ServiceId);

        if (appointmentDetail == null)
        {
            return NotFound("Không tìm thấy chi tiết lịch hẹn để xóa.");
        }

        _context.AppointmentDetails.Remove(appointmentDetail);

        var result = await _context.SaveChangesAsync();

        return result > 0
            ? Ok("Success")
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAppointmentDetailsByAppoinmentId(string id)
    {
        var appointmentdetail = await _context.AppointmentDetails
            .Include(a => a.Appointment)
            .Include(s => s.Services)
            .Where(a => a.AppointmentId == id)
            .AsNoTracking()
            .ToListAsync();
        return Ok( appointmentdetail);
    }
    

}