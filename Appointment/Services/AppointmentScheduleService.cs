using api.Appointment.Model;
using api.Data;
using Microsoft.EntityFrameworkCore;

namespace api.Appointment.Services;

public interface IAppointmentScheduleService
{
    Task<IEnumerable<AppointmentSchedule>> GetAllAsync();
    Task<AppointmentSchedule?> GetByIdAsync(string id);
    Task<IEnumerable<AppointmentSchedule>> GetByDentistIdAsync(string id);
    Task<AppointmentSchedule> CreateAsync(AppointmentSchedule schedule);
    Task<bool> UpdateAsync(AppointmentSchedule schedule);
    Task<bool> DeleteAsync(string id);
    Task InitializeMonthlySchedulesAsync();
}

public class AppointmentScheduleService : IAppointmentScheduleService
{
    private readonly ApplicationDbContext _context;

    public AppointmentScheduleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task InitializeMonthlySchedulesAsync()
    {
        var now = DateTime.Now;
        var monthsToInitialize = new[]
        {
            new DateTime(now.Year, now.Month, 1),
            new DateTime(now.Year, now.Month, 1).AddMonths(1)
        };

        var dentists = await _context.Dentists.ToListAsync();
        if (!dentists.Any()) return;

        var schedulesToAdd = new List<AppointmentSchedule>();

        foreach (var targetMonth in monthsToInitialize)
        {
            var year = targetMonth.Year;
            var month = targetMonth.Month;

            // Lấy ngày đã có lịch làm việc trong tháng này
            var existingDates = await _context.AppointmentSchedules
                .Where(s => s.Date.Value.Year == year && s.Date.Value.Month == month)
                .Select(s => new { s.DentistId, s.Date })
                .ToListAsync();

            var existingMap = new HashSet<(string dentistId, DateOnly date)>(
                existingDates.Select(e => (e.DentistId, e.Date.Value)));

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var workStart = new TimeOnly(8, 0);
            var workEnd = new TimeOnly(18, 0);

            foreach (var dentist in dentists)
            {
                for (var day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateOnly(year, month, day);
                    if (existingMap.Contains((dentist.Id, date)))
                        continue;

                    schedulesToAdd.Add(new AppointmentSchedule
                    {
                        ID = Guid.NewGuid().ToString(),
                        DentistId = dentist.Id,
                        Date = date,
                        StartTime = workStart,
                        EndTime = workEnd,
                        IsFree = true,
                        IsDayOff = false
                    });
                }
            }
        }

        if (schedulesToAdd.Any())
        {
            _context.AppointmentSchedules.AddRange(schedulesToAdd);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<AppointmentSchedule>> GetAllAsync()
    {
        return await _context.AppointmentSchedules
            .Include(a => a.Dentist)
            .ToListAsync();
    }

    public async Task<AppointmentSchedule?> GetByIdAsync(string id)
    {
        return await _context.AppointmentSchedules
            .Include(a => a.Dentist)
            .FirstOrDefaultAsync(a => a.ID == id);
    }

    public async Task<IEnumerable<AppointmentSchedule>> GetByDentistIdAsync(string dentistId)
    {
        return await _context.AppointmentSchedules
            .Where(s => s.DentistId == dentistId)
            .Include(s => s.Dentist)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<AppointmentSchedule> CreateAsync(AppointmentSchedule schedule)
    {
        _context.AppointmentSchedules.Add(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task<bool> UpdateAsync(AppointmentSchedule schedule)
    {
        var existing = await _context.AppointmentSchedules.FindAsync(schedule.ID);
        if (existing == null) return false;

        existing.Date = schedule.Date;
        existing.StartTime = schedule.StartTime;
        existing.EndTime = schedule.EndTime;
        existing.IsFree = schedule.IsFree;
        existing.IsDayOff = schedule.IsDayOff;
        existing.DentistId = schedule.DentistId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var schedule = await _context.AppointmentSchedules.FindAsync(id);
        if (schedule == null) return false;

        _context.AppointmentSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return true;
    }
}