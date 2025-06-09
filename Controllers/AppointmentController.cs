using System.Security.Claims;
using api.Data;
using api.DTOs;
using api.Models;
using api.Services.Interfaces;
using api.Services.MailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AppointmentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IDentalService _dentalService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMailService _mailService; 

    public AppointmentController(ApplicationDbContext context, IDentalService dentalService,
        UserManager<ApplicationUser> userManager, IMailService mailService)
    {
        _context = context;
        _dentalService = dentalService;
        _userManager = userManager;
        _mailService = mailService;
    }

    [HttpGet]
    public async Task<IActionResult> getAppointments()
    {
        var appointments = await _context.Appointments
            .Include(c => c.Customers)
            .Include(a => a.AppointmentDetails)
            .ThenInclude(ad => ad.Services)
            .Include(d => d.Dentists)
            .ThenInclude(u => u.User)
            .AsNoTracking()
            .ToListAsync();
        return Ok(appointments);
    }

    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> getCustomerListByDentist(string dentistId)
    {
        var completedAppointments = await _context.Appointments
            .Where(a => a.DentistId == dentistId && a.Status == AppointmentStatus.Completed)
            .Include(a => a.Customers)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(); // dùng ToListAsync thay vì AsEnumerable

        var groupedData = completedAppointments
            .GroupBy(a => new { a.CustomerId, a.Customers.FullName })
            .Select(g => new
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.FullName,
                TotalCompleted = g.Count(),
                Appointments = g.Select((a, index) => new
                {
                    VisitNumber = index + 1,
                    a.AppointmentDate,
                    a.Notes
                }).ToList()
            })
            .ToList();

        return Ok(groupedData);
    }
    [HttpGet]
    public async Task<IActionResult> GetAppointmentsByUserId(string userId)
    {
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        if (userName == null) return StatusCode(StatusCodes.Status401Unauthorized, "User not found");
        var user = await _userManager.FindByNameAsync(userName);
        if (user != null)
        {
            var appoinmentList = await _context.Appointments
                .Include(c => c.Customers)
                .Include(a => a.AppointmentDetails)
                .ThenInclude(ad => ad.Services)
                .Include(d => d.Dentists)
                .ThenInclude(u => u.User)
                .Where(c => c.CustomerId == user.Id)
                .OrderBy(a => a.AppointmentDate)
                .AsNoTracking()
                .ToListAsync();
            return Ok(appoinmentList);
        }

        return StatusCode(StatusCodes.Status401Unauthorized, "User not found");
    }

    [HttpGet]
    public async Task<IActionResult> GetPatientList(string dentistId)
    {
        var result = await _context.Appointments
            .Where(a => a.DentistId == dentistId 
                        && a.Status == AppointmentStatus.Completed 
                        && a.CustomerId != null)
            .Include(a => a.Customers)
            .GroupBy(a => a.CustomerId)
            .Select(g => new 
            {
                CustomerId = g.Key,
                FullName = g.First().Customers.FullName,
                Email = g.First().Customers.Email,
                TotalCompletedAppointments = g.Count()
            })
            .ToListAsync();
        return Ok(result);
    }
    [HttpGet]
    public async Task<IActionResult> GetPatientDetail(string id)
    {
        var appoinmentList = await _context.Appointments
            .Include(a => a.AppointmentDetails)
            .ThenInclude(ad => ad.Services)
            .Include(d => d.Dentists)
            .Where(c => c.CustomerId == id &&  c.Status == AppointmentStatus.Completed)
            .OrderBy(a => a.AppointmentDate)
            .AsNoTracking()
            .ToListAsync();
        return Ok(appoinmentList);
        
    }
    
    
    [HttpGet]
    public async Task<IActionResult> GetAppointmentsByDentistId(string dentistId)
    {
        var bookedTimes = await _context.Appointments
            .Where(a => a.DentistId == dentistId
                        && a.AppointmentDate.HasValue
                        && a.Status != AppointmentStatus.Canceled
                        )
            .Select(a => a.AppointmentDate.Value)
            
            .ToListAsync();
        return Ok(bookedTimes);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAppointmentsByDentist(string dentistId)
    {
        var appoiments = await _context.Appointments
            .Include(c => c.Customers)
            .Include(d => d.Dentists)
            .Include(a => a.AppointmentDetails)
            .ThenInclude(ad => ad.Services)
            .Where(a => a.DentistId == dentistId
                        && a.Status != AppointmentStatus.Canceled
                        && a.Status != AppointmentStatus.Pending
            )
            .OrderBy(a => a.AppointmentDate)
            .AsNoTracking()
            .ToListAsync();
        return Ok(appoiments);
    }
    [HttpGet]
    public async Task<IActionResult> getAppointmentsByStatus(AppointmentStatus status)
    {
        var appointments = await _context.Appointments
            .Include(c => c.Customers)
            .Include(d => d.Dentists)
            .Where(a => a.Status == status) // Thêm điều kiện lọc theo status
            .OrderBy(a => a.AppointmentDate)
            .AsNoTracking()
            .ToListAsync();

        return Ok(appointments);
    }


    [HttpPost]
    public async Task<IActionResult> CreateAppoimtment(AppointmentDto appointmentDto)
    {
        var newAppointment = new Appointments
        {
            AppointmentId = Guid.NewGuid().ToString(),
            DentistId = appointmentDto.DentistId,
            AppointmentDate = appointmentDto.AppointmentDate,
            CustomerId = appointmentDto.CustomerId,
            Status = AppointmentStatus.Pending,
            Notes = appointmentDto.Notes,
            CreatedAt = DateTime.Now
        };
        _context.Appointments.Add(newAppointment);
        return await _context.SaveChangesAsync() > 0
            ? Ok(newAppointment.AppointmentId)
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAppoiment([FromBody] UpdateAppointmentStatusDto dto)
    {
        var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);
        if (appointment == null) return NotFound("Appointment not found.");
        
        appointment.DentistId = dto.DentistId;
        appointment.Status = dto.Status;
        appointment.DentistNotes = dto.DentisNotes;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Status updated successfully." });
    }

    [HttpPost]
    public async Task<IActionResult> SendMailToCustomer(string email, string id, EmailType emailType)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Dentists)
            .ThenInclude(u => u.User)
            .Include(a => a.AppointmentDetails)
            .ThenInclude(ad => ad.Services)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

        if (appointment == null) return NotFound("Appointment not found.");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return NotFound("User not found.");

        string body = emailType switch
        {
            EmailType.AppointmentConfirmation => GetConfirmationBody(user, appointment),
            EmailType.AppointmentCancellation => GetCancelBody(user, appointment),
            _ => throw new ArgumentOutOfRangeException(nameof(emailType), emailType, null)
        };

        var message = new MailMessages(new[] { email }, "Thông báo từ Nha khoa", body, true);
        _mailService.SendEmail(message);

        return Ok("sent");
    }

    
    private string GetConfirmationBody(ApplicationUser user, Appointments appointment)
    {
        var mapsLink =
            "https://www.google.com/maps/place/1+%C4%90.+C%E1%BA%A7u+Gi%E1%BA%A5y,+Quan+Hoa,+C%E1%BA%A7u+Gi%E1%BA%A5y,+H%C3%A0+N%E1%BB%99i,+Vi%E1%BB%87t+Nam/@21.0313849,105.7996808,17.93z/data=!4m6!3m5!1s0x3135ab411613f95f:0x7efea5b83026932a!8m2!3d21.0313042!4d105.8010069!16s%2Fg%2F11kkpcqkn5?entry=ttu&g_ep=EgoyMDI1MDQyMy4wIKXMDSoJLDEwMjExNDU1SAFQAw%3D%3D";


        // Lấy tên nha sĩ
        string dentistName = appointment.Dentists?.User?.FullName ?? "Đang cập nhật";

        // Lấy danh sách dịch vụ
        string services = appointment.AppointmentDetails != null
            ? string.Join("<br/>", appointment.AppointmentDetails.Select(d =>
                $"- {d.Services?.ServiceName ?? "Dịch vụ không rõ"}"))
            : "Chưa có dịch vụ";

        return $@"
    <div style=""font-family: Arial, sans-serif; line-height: 1.6;"">
        <h2 style=""color: #2e6c80;"">Xác nhận lịch hẹn tại Nha khoa DENTAL CLINIC</h2>
        <p>Chào <strong>{user.FullName}</strong>,</p>
        <p>Chúng tôi đã nhận được yêu cầu đặt lịch hẹn của bạn.</p>
        <p><strong>🔹 Thời gian hẹn:</strong> {appointment.AppointmentDate:dddd, dd/MM/yyyy HH:mm}</p>
        <p><strong>🔹 Dịch vụ:</strong><br/>{services}</p>
        <p><strong>🔹 Nha sĩ:</strong> {dentistName}</p>
        <p><strong>🔹 Địa điểm:</strong> 
            <a href=""{mapsLink}"" target=""_blank"">Số 9 - Cầu Giấy - Hà Nội</a>
        </p>
        <p>Nếu bạn cần thay đổi hoặc hủy lịch hẹn, vui lòng liên hệ:</p>
        <ul>
            <li>📞 099 888 8888 - 096 666 6666</li>
        </ul>
        <p>Cảm ơn bạn đã tin tưởng sử dụng dịch vụ của chúng tôi.<br/>
        Hẹn gặp bạn đúng giờ!</p>
        <p>Trân trọng,<br/>
        <strong>Nha khoa DENTAL CLINIC</strong></p>
    </div>";
    }
    public enum EmailType
    {
        AppointmentConfirmation,
        AppointmentCancellation,
    }

    private string GetCancelBody(ApplicationUser user, Appointments appointment)
    {
        return $@"
    <div style=""font-family: Arial, sans-serif; line-height: 1.6;"">
        <h2 style=""color: #c0392b;"">Lịch hẹn của bạn đã bị hủy</h2>
        <p>Chào <strong>{user.FullName}</strong>,</p>
        <p>Lịch hẹn của bạn vào lúc <strong>{appointment.AppointmentDate:dddd, dd/MM/yyyy HH:mm}</strong> đã bị <strong>hủy</strong>.</p>
        <p>Nếu đây là sự nhầm lẫn hoặc bạn cần đặt lại lịch, vui lòng liên hệ với chúng tôi:</p>
        <ul>
            <li>📞 099 888 8888 - 096 666 6666</li>
        </ul>
        <p>Chúng tôi xin lỗi vì sự bất tiện này.</p>
        <p>Trân trọng,<br/>
        <strong>Nha khoa DENTAL CLINIC</strong></p>
    </div>";
    }


}