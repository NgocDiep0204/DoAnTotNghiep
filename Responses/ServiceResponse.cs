namespace api.Responses;

public class ServiceResponse
{
    public record class RegisterResponse(bool Flag, string? Message, string? userId);
    public record class LoginResponse(bool Flag, string Message, string? Token);
}