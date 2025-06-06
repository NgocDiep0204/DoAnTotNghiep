using api.Data;
using api.DTOs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ServiceStepController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ServiceStepController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateServiceStep(ServiceStepDto serviceStepDto)
    {
        var newStep = new ServiceSteps
        {
            Id = Guid.NewGuid().ToString(),
            ServiceId = serviceStepDto.ServiceId,
            Title = serviceStepDto.Title,
            Description = serviceStepDto.Description,
            CreatedAt = DateTime.Now,
        };
        _context.ServiceSteps.Add(newStep);
        return await _context.SaveChangesAsync() > 0
            ? StatusCode(StatusCodes.Status200OK, "Success")
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }

    [HttpPut]
    public async Task<IActionResult> UpdateServiceStep(ServiceStepDto serviceStepDto)
    {
        var serviceStep = await _context.ServiceSteps.FindAsync(serviceStepDto.Id);
        if (serviceStep == null)
            return StatusCode(StatusCodes.Status404NotFound, "Service step not found");
        serviceStep.Title = serviceStepDto.Title;
        serviceStep.Description = serviceStepDto.Description;
        return await _context.SaveChangesAsync() > 0
            ? StatusCode(StatusCodes.Status200OK, "Success")
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteServiceStep(string id)
    {
        var serviceSteps = await _context.ServiceSteps
            .Where(c => c.ServiceId == id)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        if (serviceSteps == null || serviceSteps.Count == 0)
            return NotFound(new { message = "Service steps not found." });

        _context.ServiceSteps.RemoveRange(serviceSteps);

        return await _context.SaveChangesAsync() > 0
            ? Ok("Deleted successfully.")
            : StatusCode(StatusCodes.Status500InternalServerError, "Delete failed.");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteServiceStepByIdStep(string id)
    {
        var serviceSteps = await _context.ServiceSteps.FindAsync(id);

        if (serviceSteps == null)
            return NotFound(new { message = "Service steps not found." });

        _context.ServiceSteps.Remove(serviceSteps);

        return await _context.SaveChangesAsync() > 0
            ? Ok("Deleted successfully.")
            : StatusCode(StatusCodes.Status500InternalServerError, "Delete failed.");
    }
}