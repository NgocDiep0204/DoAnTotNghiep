using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ServiceController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ServiceController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllServices()
    {
        var allServices = await _context.DentalServices.ToListAsync();
        return Ok(allServices);
    }


    [HttpGet]
    public async Task<IActionResult> GetServiceByID(string id)
    {
        var serviceById = await _context.DentalServices
            .Where(c => c.ServiceId == id)
            .FirstOrDefaultAsync();

        if (serviceById == null) return NotFound();

        return Ok(serviceById);
    }
}