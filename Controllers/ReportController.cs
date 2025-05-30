using api.Data;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]/[action]")]
public class ReportController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Helper dùng chung để lọc theo year và optional month
    private IQueryable<Appointments> FilterAppointments(int? year, int? month, DateTime? from, DateTime? to)
    {
        IQueryable<Appointments> query = _context.Appointments;

        if (from.HasValue && to.HasValue)
        {
            // Nếu from và to cùng ngày => lọc theo ngày
            if (from.Value.Date == to.Value.Date)
            {
                var targetDate = from.Value.Date;
                query = query.Where(a => a.AppointmentDate.HasValue && a.AppointmentDate.Value.Date == targetDate);
            }
            else
            {
                query = query.Where(a =>
                    a.AppointmentDate.HasValue &&
                    a.AppointmentDate.Value.Date >= from.Value.Date &&
                    a.AppointmentDate.Value.Date <= to.Value.Date);
            }
        }
        else
        {
            int targetYear = year ?? DateTime.Now.Year;
            query = query.Where(a => a.AppointmentDate.HasValue && a.AppointmentDate.Value.Year == targetYear);

            if (month.HasValue)
            {
                query = query.Where(a => a.AppointmentDate.Value.Month == month.Value);
            }
        }

        return query;
    }



    [HttpGet]
    public async Task<IActionResult> GetOverviewAppointment(int? year = null, int? month = null, DateTime? from = null, DateTime? to = null)
    {
        var query = FilterAppointments(year, month, from, to);
        var appointments = await query.ToListAsync();

        var overview = new
        {
            Year = year,
            Month = month,
            From = from,
            To = to,
            Total = appointments.Count,
            Confirmed = appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
            Cancelled = appointments.Count(a => a.Status == AppointmentStatus.Canceled),
            Pending = appointments.Count(a => a.Status == AppointmentStatus.Pending),
            Completed = appointments.Count(a => a.Status == AppointmentStatus.Completed)
        };

        var groupedByDate = appointments
            .GroupBy(a => a.AppointmentDate.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Total = g.Count(),
                Confirmed = g.Count(a => a.Status == AppointmentStatus.Confirmed),
                Cancelled = g.Count(a => a.Status == AppointmentStatus.Canceled),
                Pending = g.Count(a => a.Status == AppointmentStatus.Pending),
                Completed = g.Count(a => a.Status == AppointmentStatus.Completed)
            })
            .OrderBy(x => x.Date)
            .ToList();

        return Ok(new
        {
            Overview = overview,
            Statistics = groupedByDate
        });
    }



    
    [HttpGet]
    public async Task<IActionResult> GetByDoctor(int? year = null, int? month = null, DateTime? from = null, DateTime? to = null)
    {
        var filteredAppointments = FilterAppointments(year, month, from, to);

        var result = await _context.Dentists
            .Include(d => d.User)
            .GroupJoin(
                filteredAppointments,
                dentist => dentist.Id,
                appointment => appointment.DentistId,
                (dentist, appts) => new
                {
                    DoctorName = dentist.User.FullName,
                    TotalAppointments = appts.Count(),
                    Completed = appts.Count(a => a.Status == AppointmentStatus.Completed),
                    Cancelled = appts.Count(a => a.Status == AppointmentStatus.Canceled),
                }).ToListAsync();

        return Ok(new
        {
            Year = year,
            Month = month,
            From = from,
            To = to,
            Data = result
        });
    }



    [HttpGet]
    public async Task<IActionResult> GetByService(int? year = null, int? month = null, DateTime? from = null, DateTime? to = null)
    {
        var filteredAppointments = FilterAppointments(year, month, from, to);
        var appointmentIds = await filteredAppointments.Select(a => a.AppointmentId).ToListAsync();

        var appointmentDetailsQuery = _context.AppointmentDetails
            .Where(ad => appointmentIds.Contains(ad.AppointmentId));

        var result = await _context.DentalServices
            .GroupJoin(
                appointmentDetailsQuery,
                s => s.ServiceId,
                ad => ad.ServiceId,
                (s, adGroup) => new
                {
                    ServiceName = s.ServiceName,
                    TotalUsed = adGroup.Count()
                })
            .ToListAsync();

        return Ok(new
        {
            Year = year,
            Month = month,
            From = from,
            To = to,
            Data = result
        });
    }



    [HttpGet]
    public async Task<IActionResult> GetAppointmentsByCustomer(int? year = null, int? month = null, DateTime? from = null, DateTime? to = null)
    {
        var customers = await _userManager.GetUsersInRoleAsync("User");
        var customerIds = customers.Select(c => c.Id).ToList();

        var query = FilterAppointments(year, month, from, to)
            .Include(a => a.Customers)
            .Where(a => customerIds.Contains(a.CustomerId));

        var result = await query
            .GroupBy(a => a.Customers.FullName)
            .Select(g => new
            {
                CustomerName = g.Key,
                TotalAppointments = g.Count(),
                Confirmed = g.Count(a => a.Status == AppointmentStatus.Confirmed),
                Completed = g.Count(a => a.Status == AppointmentStatus.Completed),
                Cancelled = g.Count(a => a.Status == AppointmentStatus.Canceled),
                Pending = g.Count(a => a.Status == AppointmentStatus.Pending)
            })
            .ToListAsync();

        return Ok(new
        {
            Year = year,
            Month = month,
            From = from,
            To = to,
            Data = result
        });
    }

}
