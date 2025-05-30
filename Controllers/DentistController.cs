using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class DentistController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DentistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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
    public async Task<IActionResult> GetDentistsByStatus()
    {
        var activeDentist = _context.Dentists
            .Include(c => c.User)
            .Where(c => c.Status == Status.active);
        return Ok(activeDentist);
    }

    [HttpGet]
    public async Task<IActionResult> GetDentistById(string id)
    {
        var dentist = await _context.Dentists
            .Include(c => c.User)
            .FirstOrDefaultAsync(d => d.Id == id);
        return Ok(dentist);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDentist(DentistDto dentist)
    {
        var newDentist = new Dentists
        {
            Id = Guid.NewGuid().ToString(),
            UserId = dentist.UserId,
            Years = dentist.Years,
            Education = dentist.Education,
            Introduce = dentist.Introduce,
            Certificate = dentist.Certificate,
            Speacialty = dentist.Speacialty,
            Price = dentist.Price,
            Postgraduates = dentist.Postgraduates,
            Status = Status.active
        };
        _context.Dentists.Add(newDentist);
        return await _context.SaveChangesAsync() > 0
            ? StatusCode(StatusCodes.Status200OK, "Success")
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDentist(DentistDto dentistDto)
    {
        var dentist = await _context.Dentists.FirstOrDefaultAsync(d => d.Id == dentistDto.Id);
        if (dentist == null)
            return NotFound();
        dentist.Speacialty = dentistDto.Speacialty;
        dentist.Status = dentistDto.Status;
        dentist.Introduce = dentistDto.Introduce;
        dentist.Years = dentistDto.Years;
        dentist.Education = dentistDto.Education;
        dentist.Certificate = dentistDto.Certificate;
        dentist.Price = dentistDto.Price;
        dentist.Postgraduates = dentistDto.Postgraduates;
        _context.Dentists.Update(dentist);

        return await _context.SaveChangesAsync() > 0
            ? StatusCode(StatusCodes.Status200OK, "Success")
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteDentist(string id, string userId)
    {
        var dentist = await _context.Dentists.FirstOrDefaultAsync(d => d.Id == id);
        var user = await _userManager.FindByIdAsync(userId);

        if (dentist == null || user == null)
            return NotFound("Dentist hoặc User không tồn tại");

        _context.Dentists.Remove(dentist);
        await _context.SaveChangesAsync();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }
}