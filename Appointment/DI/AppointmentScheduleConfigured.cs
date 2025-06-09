using api.Appointment.Services;

namespace api.Appointment.DI;

public static class AppointmentScheduleConfigured
{
    public static void AddAppointmentScheduleScope(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAppointmentScheduleService, AppointmentScheduleService>();
    }

    public static async void InitializeAppointmentSchedule(this WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentScheduleService>();
            await appointmentService.InitializeMonthlySchedulesAsync();
        }
        catch (Exception e)
        {
            throw e;
        }
    }
}