namespace IrrigationMaster.Mobile.Application.Features.Models.Auth
{
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
