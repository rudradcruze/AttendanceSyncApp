namespace AttandanceSyncApp.Helpers
{
    /// <summary>
    /// Helper class for converting status codes to human-readable text and CSS classes
    /// </summary>
    public static class StatusHelper
    {
        /// <summary>
        /// Converts a boolean status (null/true/false) to human-readable text
        /// </summary>
        public static string GetStatusText(bool? status)
        {
            if (!status.HasValue) return "Pending";
            return status.Value ? "Completed" : "Failed";
        }

        /// <summary>
        /// Converts a status code (NR, IP, CP, RR, CN) to human-readable text
        /// </summary>
        public static string GetStatusTextFromCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return "Unknown";

            switch (code.ToUpper())
            {
                case "NR": return "New Request";
                case "IP": return "In Progress";
                case "CP": return "Completed";
                case "RR": return "Rejected";
                case "CN": return "Cancelled";
                default: return code;
            }
        }

        /// <summary>
        /// Gets CSS class for status code styling
        /// </summary>
        public static string GetStatusClass(string code)
        {
            if (string.IsNullOrEmpty(code)) return "";

            switch (code.ToUpper())
            {
                case "NR": return "status-nr";
                case "IP": return "status-ip";
                case "CP": return "status-cp";
                case "RR": return "status-rr";
                case "CN": return "status-cn";
                default: return "";
            }
        }

        /// <summary>
        /// Gets Bootstrap badge class for status
        /// </summary>
        public static string GetStatusBadgeClass(string code)
        {
            if (string.IsNullOrEmpty(code)) return "bg-secondary";

            switch (code.ToUpper())
            {
                case "NR": return "bg-info";
                case "IP": return "bg-warning";
                case "CP": return "bg-success";
                case "RR": return "bg-danger";
                case "CN": return "bg-secondary";
                default: return "bg-secondary";
            }
        }

        /// <summary>
        /// Checks if status indicates a final state (cannot be changed)
        /// </summary>
        public static bool IsFinalStatus(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;

            var upperCode = code.ToUpper();
            return upperCode == "CP" || upperCode == "RR" || upperCode == "CN";
        }

        /// <summary>
        /// Checks if status allows processing
        /// </summary>
        public static bool CanProcess(string code, bool isCancelled = false)
        {
            if (isCancelled) return false;
            return !IsFinalStatus(code);
        }
    }
}
