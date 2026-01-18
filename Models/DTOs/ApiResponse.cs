using System;
using System.Collections.Generic;

namespace AttandanceSyncApp.Models.DTOs
{
    /// <summary>
    /// Standard API response wrapper without data payload
    /// </summary>
    public class ApiResponse
    {
        public string Timestamp { get; set; }
        public List<string> Errors { get; set; }
        public string Message { get; set; }

        public static ApiResponse Success(string message = null)
        {
            return new ApiResponse
            {
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Errors = null,
                Message = message ?? "Success"
            };
        }

        public static ApiResponse Fail(string error, string message = null)
        {
            return new ApiResponse
            {
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Errors = new List<string> { error },
                Message = message ?? "Operation failed"
            };
        }

        public static ApiResponse Fail(List<string> errors, string message = null)
        {
            return new ApiResponse
            {
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Errors = errors,
                Message = message ?? "Operation failed"
            };
        }
    }

    /// <summary>
    /// Standard API response wrapper with typed data payload
    /// </summary>
    public class ApiResponse<T>
    {
        public string Timestamp { get; set; }
        public List<string> Errors { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }

        public static ApiResponse<T> Success(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Errors = null,
                Data = data,
                Message = message ?? "Success"
            };
        }

        public static ApiResponse<T> Fail(string error, string message = null)
        {
            return new ApiResponse<T>
            {
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Errors = new List<string> { error },
                Data = default(T),
                Message = message ?? "Operation failed"
            };
        }

        public static ApiResponse<T> Fail(List<string> errors, string message = null)
        {
            return new ApiResponse<T>
            {
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Errors = errors,
                Data = default(T),
                Message = message ?? "Operation failed"
            };
        }
    }
}
