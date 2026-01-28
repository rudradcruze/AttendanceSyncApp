namespace AttandanceSyncApp.Helpers
{
    /// <summary>
    /// Helper class for converting status codes to human-readable text and CSS classes.
    /// Supports status codes: NR (New Request), IP (In Progress), CP (Completed),
    /// RR (Rejected), and CN (Cancelled).
    /// </summary>
    public static class StatusHelper
    {
        /// <summary>
        /// Converts a nullable boolean status to human-readable text.
        /// </summary>
        /// <param name="status">The status value (null=Pending, true=Completed, false=Failed).</param>
        /// <returns>Status text: "Pending", "Completed", or "Failed".</returns>
        public static string GetStatusText(bool? status)
        {
            // Null means the operation hasn't completed yet
            if (!status.HasValue) return "Pending";
            // True means success, false means failure
            return status.Value ? "Completed" : "Failed";
        }

        /// <summary>
        /// Converts a status code to human-readable text.
        /// </summary>
        /// <param name="code">The status code (NR, IP, CP, RR, CN).</param>
        /// <returns>Human-readable status text.</returns>
        public static string GetStatusTextFromCode(string code)
        {
            // Return "Unknown" for empty or null codes
            if (string.IsNullOrEmpty(code)) return "Unknown";

            // Map status codes to readable text
            switch (code.ToUpper())
            {
                case "NR": return "New Request";      // Initial state when request is created
                case "IP": return "In Progress";      // Request is being processed
                case "CP": return "Completed";        // Successfully completed
                case "RR": return "Rejected";         // Rejected by admin
                case "CN": return "Cancelled";        // Cancelled by user
                default: return code;                 // Return original code if unrecognized
            }
        }

        /// <summary>
        /// Gets custom CSS class for status code styling.
        /// </summary>
        /// <param name="code">The status code.</param>
        /// <returns>CSS class name for the status.</returns>
        public static string GetStatusClass(string code)
        {
            // Return empty string for null/empty codes
            if (string.IsNullOrEmpty(code)) return "";

            // Map status codes to custom CSS classes
            switch (code.ToUpper())
            {
                case "NR": return "status-nr";  // New Request styling
                case "IP": return "status-ip";  // In Progress styling
                case "CP": return "status-cp";  // Completed styling
                case "RR": return "status-rr";  // Rejected styling
                case "CN": return "status-cn";  // Cancelled styling
                default: return "";
            }
        }

        /// <summary>
        /// Gets Bootstrap badge CSS class for status display.
        /// </summary>
        /// <param name="code">The status code.</param>
        /// <returns>Bootstrap badge class (bg-info, bg-warning, bg-success, bg-danger, bg-secondary).</returns>
        public static string GetStatusBadgeClass(string code)
        {
            // Default to secondary (gray) for unknown statuses
            if (string.IsNullOrEmpty(code)) return "bg-secondary";

            // Map status codes to Bootstrap badge colors
            switch (code.ToUpper())
            {
                case "NR": return "bg-info";       // Blue for new requests
                case "IP": return "bg-warning";    // Yellow for in-progress
                case "CP": return "bg-success";    // Green for completed
                case "RR": return "bg-danger";     // Red for rejected
                case "CN": return "bg-secondary";  // Gray for cancelled
                default: return "bg-secondary";    // Gray for unknown
            }
        }

        /// <summary>
        /// Checks if status indicates a final state that cannot be modified.
        /// </summary>
        /// <param name="code">The status code.</param>
        /// <returns>True if status is final (CP, RR, or CN), false otherwise.</returns>
        public static bool IsFinalStatus(string code)
        {
            // Return false for null/empty codes
            if (string.IsNullOrEmpty(code)) return false;

            // Final states are: Completed, Rejected, or Cancelled
            var upperCode = code.ToUpper();
            return upperCode == "CP" || upperCode == "RR" || upperCode == "CN";
        }

        /// <summary>
        /// Checks if a request with the given status can be processed.
        /// </summary>
        /// <param name="code">The status code.</param>
        /// <param name="isCancelled">Whether the request has been explicitly cancelled.</param>
        /// <returns>True if the request can be processed, false if in final state or cancelled.</returns>
        public static bool CanProcess(string code, bool isCancelled = false)
        {
            // Cancelled requests cannot be processed
            if (isCancelled) return false;
            // Cannot process requests in final states
            return !IsFinalStatus(code);
        }
    }
}
