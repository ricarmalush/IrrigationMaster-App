namespace IrrigationMaster.Mobile.Application.Features.Models.Structure;

// Espejo de RegenerateInvitationCodeCommand del backend (PUT Organizations/RegenerateInvitationCode/{id}).
// OrganizationId va también en la URL -- el controller lo sobreescribe ahí (command with
// { OrganizationId = id }), así que el valor del body es en la práctica ignorado, pero se manda
// igual para que el shape del JSON coincida con el Command. CustomCode siempre null desde la App
// (igual que Angular): no se expone edición manual del código, solo regenerar uno nuevo.
public class RegenerateInvitationCodeRequest
{
    public Guid OrganizationId { get; set; }
    public string? CustomCode { get; set; }
}
