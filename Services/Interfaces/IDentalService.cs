namespace api.Services.Interfaces;

public interface IDentalService
{
    Task<object> GetServicesAsync(string id);
}