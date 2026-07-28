namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo de AssignWalkwayCommand del backend (Users/AssignWalkway/{id}). WalkwayId null quita
// la asignación.
public class AssignWalkwayRequest
{
    public Guid? WalkwayId { get; set; }
}
