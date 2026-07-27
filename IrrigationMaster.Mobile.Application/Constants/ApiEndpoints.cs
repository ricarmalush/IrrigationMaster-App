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
        public const string HydraulicSectors = "hydraulicsectors/Create";
        public const string Walkways = "walkways/Create";
        public const string Countries = "Countries/pagination";
    }
}
