//using api.Validations;

namespace api.DTOs.Authentication;

public class ChangePasswordDto
{
    //[Required(ErrorMessage = "Emtpy Password")]
    public required string CurrentPassword { get; set; }

    //[PasswordStrength]
    //[Required(ErrorMessage = "Emtpy Password")]
    public required string NewPassword { get; set; }
}