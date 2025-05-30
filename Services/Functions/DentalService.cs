using api.Data;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Functions;

public class DentalService : IDentalService
{
    private readonly ApplicationDbContext _context;

    public DentalService(ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<object> GetServicesAsync(string id)
    {
        var query = _context.Appointments
            .Include(u => u.Customers)
            .Include(d => d.Dentists)
            .ThenInclude(c => c.User)
            .Where(a => a.CustomerId == id || a.DentistId == id)
            .GroupJoin(_context.AppointmentDetails,
                a => a.AppointmentId,
                ad => ad.AppointmentId,
                (a, appointmentDetails) => new { a, appointmentDetails })

            // LEFT JOIN với DentalServices để lấy thông tin dịch vụ nha khoa
            .SelectMany(x => x.appointmentDetails.DefaultIfEmpty(),
                (x, ad) => new { x.a, appointmentDetails = ad })
            .GroupJoin(_context.DentalServices,
                ad => ad.appointmentDetails!.ServiceId,
                ds => ds.ServiceId,
                (ad, dentalServices) => new { ad.a, ad.appointmentDetails, DentalService = dentalServices })
            .SelectMany(x => x.DentalService.DefaultIfEmpty(),
                (x, ds) => new
                {
                    x.a.AppointmentId,
                    x.a.CustomerId,
                    x.a.DentistId,
                    x.a.Status,
                    x.a.AppointmentDate,
                    ServiceName = ds != null ? ds.ServiceName : null,
                    CustomerName = x.a.Customers.FullName,
                    DentistName = x.a.Dentists.User.FullName,
                })
            .GroupBy(x => new { x.AppointmentId, x.CustomerId, x.DentistId, x.Status, x.AppointmentDate })
            .Select(g => new
            {
                g.Key.AppointmentId,
                g.Key.CustomerId,
                g.Key.Status,
                g.Key.AppointmentDate,
                g.FirstOrDefault().CustomerName,
                g.Key.DentistId,
                g.FirstOrDefault().DentistName,
                g.FirstOrDefault().ServiceName,
            });

        return query;
    }
}