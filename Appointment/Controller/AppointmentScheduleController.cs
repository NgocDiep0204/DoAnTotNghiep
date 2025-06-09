using System.Runtime.CompilerServices;
using api.Appointment.Model;
using api.Appointment.Services;
using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Appointment.Controller;

[ApiController]
[Route("api/[controller]/[action]")]
public class AppointmentScheduleController : ControllerBase
{
    private readonly IAppointmentScheduleService _service;
    private readonly ApplicationDbContext _context;

    public AppointmentScheduleController(IAppointmentScheduleService service, ApplicationDbContext context)
    {
        _service = service;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var schedules = await _service.GetAllAsync();
        return Ok(schedules);
    }

    [HttpGet]
    public async Task<IActionResult> GetById([FromQuery] string id)
    {
        var schedule = await _service.GetByDentistIdAsync(id);
        if (schedule == null) return NotFound();
        return Ok(schedule);
    }

    [HttpPost]
    public async Task<IActionResult> CreateForMonth([FromBody] CreateScheduleForMonthDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var dentistExists = await _context.Dentists.AnyAsync(d => d.Id == dto.DentistId);
        if (!dentistExists)
            return NotFound($"Dentist với ID {dto.DentistId} không tồn tại.");

        int daysInMonth = DateTime.DaysInMonth(dto.Year, dto.Month);
        var schedules = new List<AppointmentSchedule>();

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(dto.Year, dto.Month, day);

            var schedule = new AppointmentSchedule
            {
                ID = Guid.NewGuid().ToString(),
                DentistId = dto.DentistId,
                Date = date,
                StartTime = new TimeOnly(8, 0), // 08:00
                EndTime = new TimeOnly(18, 0), // 18:00
                IsFree = true,
                IsDayOff = false
            };

            schedules.Add(schedule);
        }

        _context.AppointmentSchedules.AddRange(schedules);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Tạo lịch thành công", Count = schedules.Count });
    }


    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AppointmentScheduleDto dto)
    {
        //if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _service.GetByIdAsync(dto.ID);
        if (existing == null) return NotFound();


        existing.DentistId = dto.DentistId;
        existing.Date = dto.Date;
        existing.StartTime = dto.StartTime;
        existing.EndTime = dto.EndTime;
        existing.IsFree = dto.IsFree;
        existing.IsDayOff = dto.IsDayOff;

        _context.Update(existing);
        var res = await _context.SaveChangesAsync();
        if (res != 1) return NoContent();
        return Ok("sucess");
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

public class AppointmentScheduleDto
{
    public string ID { get; set; }
    public string DentistId { get; set; }
    public DateOnly? Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool? IsFree { get; set; } = true;
    public bool? IsDayOff { get; set; } = false;
}

public class CreateScheduleForMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; } // 1–12
    public string DentistId { get; set; }
}