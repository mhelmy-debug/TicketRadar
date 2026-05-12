// File: Services/TicketStatusStore.cs
namespace TicketRadar.Services
{
    public static class TicketStatusStore
    {
        private static readonly object LockObject = new();

        private static string _selectedTeam = "";
        private static string _lastStatus = "اختر النادي واضغط تشغيل الرادار";
        private static DateTime? _lastCheckTime;
        private static string _lastBookingUrl = "https://www.tazkarti.com/#/matches";

        public static string SelectedTeam
        {
            get { lock (LockObject) { return _selectedTeam; } }
            set { lock (LockObject) { _selectedTeam = value ?? ""; } }
        }

        public static string LastStatus
        {
            get { lock (LockObject) { return _lastStatus; } }
            set { lock (LockObject) { _lastStatus = value ?? "اختر النادي واضغط تشغيل الرادار"; } }
        }

        public static DateTime? LastCheckTime
        {
            get { lock (LockObject) { return _lastCheckTime; } }
            set { lock (LockObject) { _lastCheckTime = value; } }
        }

        public static string LastBookingUrl
        {
            get { lock (LockObject) { return _lastBookingUrl; } }
            set { lock (LockObject) { _lastBookingUrl = string.IsNullOrWhiteSpace(value) ? "https://www.tazkarti.com/#/matches" : value; } }
        }
    }
}