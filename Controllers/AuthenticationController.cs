using api.DTOs.Authentication;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authenticationService.RegisterAsync(registerDto);
        if (result.Flag == false) return StatusCode(StatusCodes.Status400BadRequest, result.Message);

        return Ok(new { result.userId });
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _authenticationService.LoginAsync(loginDto);
        if (result.Flag == false) return StatusCode(StatusCodes.Status400BadRequest, result.Message);
        return Ok(new { result.Token });
    }
}