using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationMaster.Mobile.Application.Constants
{
    public static class ApiEndpoints
    {
        public const string Login = "Auth/Login";
        public const string Organizations = "organizations/Create";
        public const string OrganizationsGet = "organizations/Get";
        public const string OrganizationsPagination = "organizations/pagination";
        public const string OrganizationsRegenerateInvitationCode = "organizations/RegenerateInvitationCode";
        public const string HydraulicSectors = "hydraulicsectors/Create";
        public const string HydraulicSectorsPagination = "hydraulicsectors/pagination";
        public const string Walkways = "walkways/Create";
        public const string WalkwaysPublic = "walkways/public";
        public const string Countries = "Countries/pagination";
        // Auto-registro público, anónimo. Ruta separada de "Users/Create" (que ahora exige un
        // caller autenticado con permiso) tras el diagnóstico de seguridad: un único endpoint
        // condicional según currentUser.IsAuthenticated permitía que un HttpClient con un token de
        // otra sesión creara usuarios ya Activos sin pasar por aprobación.
        public const string UsersRegister = "Users/Register";
        public const string UsersPagination = "Users/pagination";
        public const string UsersActivate = "Users/Activate";
        public const string UsersDeactivate = "Users/Deactivate";
        public const string UsersAssignWalkway = "Users/AssignWalkway";
        public const string UsersChangeRole = "Users/ChangeRole";
        public const string UsersChangePassword = "Users/ChangePassword";
        public const string UsersResetPassword = "Users/ResetPassword";
        public const string RolesPagination = "Roles/pagination";
        public const string WalkwaysPagination = "walkways/pagination";
        public const string WalkwaysGet = "walkways/Get";
        public const string IrrigationTurns = "IrrigationTurns";
        public const string IrrigationTurnsCreate = "IrrigationTurns/Create";
        public const string IrrigationTurnsStatus = "IrrigationTurns/status";
        public const string IrrigationTurnsMyWalkwayStatus = "IrrigationTurns/my-walkway-status";
        public const string IrrigationTurnsPendingApproval = "IrrigationTurns/pending-approval";
        public const string IrrigationProgramsIsIrrigationDay = "IrrigationPrograms/IsIrrigationDay";
        public const string IrrigationProgramsPagination = "IrrigationPrograms/pagination";
        public const string IrrigationProgramsCreate = "IrrigationPrograms/Create";
        public const string IrrigationProgramsUpdate = "IrrigationPrograms/Update";
        public const string NotificationsMine = "Notifications/Mine";
        public const string NotificationsMarkAsRead = "Notifications/MarkAsRead";
        public const string NotificationsMarkAllAsRead = "Notifications/MarkAllAsRead";
        public const string NotificationsReportIncident = "Notifications/ReportIncident";
        public const string NotificationsSend = "Notifications/Send";
        public const string UsersGet = "Users/Get";
    }
}
