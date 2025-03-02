using api.DTOs.Authentication;
using api.Responses;

namespace api.Services.Interfaces;

public interface IAuthenticationService
{
    Task<ServiceResponse.RegisterResponse> RegisterAsync(RegisterDto registerDto);

    Task<ServiceResponse.LoginResponse> LoginAsync(LoginDto loginDto);
    public string GenerateRandomOtp();
    Task RemoveExpiredOtps();
}