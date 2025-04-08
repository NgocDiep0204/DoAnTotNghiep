using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class DentistController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DentistController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetDentists()
    {
        var dentits = await _context.Dentists
            .Include(c => c.User)
            .ToListAsync();
        return Ok(dentits);
    }

    [HttpGet]
    public async Task<IActionResult> GetDentistById(string id)
    {
        var dentist = await _context.Dentists
            .Include(c => c.User)
            .FirstOrDefaultAsync(d => d.Id == id);
        return Ok(dentist);
    }
}