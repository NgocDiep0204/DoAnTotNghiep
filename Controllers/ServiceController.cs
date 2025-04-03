using api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]/[action]   ")]
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
            var allServices = _context.DentalServices.ToList();
            return Ok(allServices);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllServicesByID(string  id)
        {
            return Ok(await _context.DentalServices.FindAsync(id));
        }
        
        
    }
}
