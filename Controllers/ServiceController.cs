using System.Net;
using api.Data;
using api.DTOs;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ServiceController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;

    public ServiceController(ApplicationDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllServices()
    {
        var allServices = await _context.DentalServices
            .Include(c => c.ServiceSteps)
            .AsNoTracking()
            .ToListAsync();
        return Ok(allServices);
    }


    [HttpGet]
    public async Task<IActionResult> GetServiceByID(string id)
    {
        var serviceById = await _context.DentalServices
            .Include(c => c.ServiceSteps)
            .Where(c => c.ServiceId == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (serviceById == null) return NotFound();

        return Ok(serviceById);
    }

    [HttpGet]
    public async Task<IActionResult> GetServicesByDentalStatus(Status status)
    {
        var serviceByStatus = await _context.DentalServices
            .Include(c => c.ServiceSteps)
            .Where(c => c.Status == status)
            .AsNoTracking()
            .ToListAsync();

        if (serviceByStatus == null) return NotFound();

        return Ok(serviceByStatus);
    }

    [HttpPost]
    public async Task<IActionResult> AddService([FromForm] DentalServiceDto dentalService)
    {
        string? imgPath = null;
        if (dentalService.FormFile != null)
        {
            var uploadResult = await _imageService.AddImageAsync(dentalService.FormFile);
            if (uploadResult.StatusCode == HttpStatusCode.OK)
                imgPath = uploadResult.SecureUrl.AbsoluteUri;
            else
                return StatusCode((int)uploadResult.StatusCode, "Image upload failed.");
        }

        var newService = new DentalServices
        {
            ServiceId = Guid.NewGuid().ToString(),
            ServiceName = dentalService.ServiceName,
            ServiceDescription = dentalService.ServiceDescription,
            Benefit = dentalService.Benefit,
            Price = dentalService.Price,
            Status = dentalService.Status,
            CreatedAt = DateTime.Now,
            Duration = dentalService.Duration,
            ImgService = imgPath
        };
        _context.DentalServices.Add(newService);
        return await _context.SaveChangesAsync() > 0
            ? StatusCode(StatusCodes.Status200OK, "Success")
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }


    [HttpPut]
    public async Task<IActionResult> UpdateService([FromForm] DentalServiceDto dentalService)
    {
        var existService = await _context.DentalServices.FindAsync(dentalService.ServiceId);

        if (existService == null) return NotFound(new { message = "Service not found." });

        string? imgPath = null;
        if (dentalService.FormFile != null)
        {
            var uploadResult = await _imageService.AddImageAsync(dentalService.FormFile);
            if (uploadResult.StatusCode == HttpStatusCode.OK)
                imgPath = uploadResult.SecureUrl.AbsoluteUri;
            else
                return StatusCode((int)uploadResult.StatusCode, "Image upload failed.");
        }

        existService.ServiceName = dentalService.ServiceName ?? existService.ServiceName;
        existService.ServiceDescription = dentalService.ServiceDescription ?? existService.ServiceDescription;
        existService.Benefit = dentalService.Benefit ?? existService.Benefit;
        existService.Price = dentalService.Price ?? existService.Price;
        existService.Status = dentalService.Status ?? existService.Status;
        existService.Duration = dentalService.Duration ?? existService.Duration;
        existService.ImgService = imgPath ?? existService.ImgService;

        _context.DentalServices.Update(existService);
        return await _context.SaveChangesAsync() > 0
            ? StatusCode(StatusCodes.Status200OK, "Success")
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteService(string id)
    {
        var service = await _context.DentalServices.FindAsync(id);
        if (service == null) return NotFound(new { message = "Service not found." });
        _context.DentalServices.Remove(service);
        return await _context.SaveChangesAsync() > 0
            ? StatusCode(StatusCodes.Status200OK, "Success")
            : StatusCode(StatusCodes.Status500InternalServerError, "Error");
    }
}