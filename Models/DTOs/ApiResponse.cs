using System;
using System.Collections.Generic;

namespace AttandanceSyncApp.Models.DTOs
{
    /// <summary>
    /// Standard API response wrapper without data payload
    /// </summary>
    public class ApiResponse
    {
        public DateTime Timestamp { get; set; }
        public List<string> Errors { get; set; }
        public string Message { get; set; }

        public static ApiResponse Success(string message = null)
        {
            return new ApiResponse
            {
                Timestamp = DateTime.UtcNow,
                Errors = null,
                Message = message
            };
        }

        public static ApiResponse Fail(string error, string message = null)
        {
            return new ApiResponse
            {
                Timestamp = DateTime.UtcNow,
                Errors = new List<string> { error },
                Message = message ?? error
            };
        }

        public static ApiResponse Fail(List<string> errors, string message = null)
        {
            return new ApiResponse
            {
                Timestamp = DateTime.UtcNow,
                Errors = errors,
                Message = message ?? (errors != null && errors.Count > 0 ? errors[0] : null)
            };
        }
    }

    /// <summary>
    /// Standard API response wrapper with typed data payload
    /// </summary>
    public class ApiResponse<T>
    {
        public DateTime Timestamp { get; set; }
        public List<string> Errors { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }

        public static ApiResponse<T> Success(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Timestamp = DateTime.UtcNow,
                Errors = null,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> Fail(string error, string message = null)
        {
            return new ApiResponse<T>
            {
                Timestamp = DateTime.UtcNow,
                Errors = new List<string> { error },
                Data = default(T),
                Message = message ?? error
            };
        }

        public static ApiResponse<T> Fail(List<string> errors, string message = null)
        {
            return new ApiResponse<T>
            {
                Timestamp = DateTime.UtcNow,
                Errors = errors,
                Data = default(T),
                Message = message ?? (errors != null && errors.Count > 0 ? errors[0] : null)
            };
        }
    }
}
