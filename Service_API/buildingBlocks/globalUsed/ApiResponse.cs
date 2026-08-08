namespace Service_API.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public int StatusCode { get; set; }

        public static ApiResponse<T> SuccessResult(T data, string message = "Operation completed successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 200
            };
        }

        public static ApiResponse<T> ErrorResult(string message, int statusCode = 400, List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                Errors = errors ?? new List<string>(),
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> NotFoundResult(string message = "Resource not found")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                StatusCode = 404
            };
        }

        public static ApiResponse<T> UnauthorizedResult(string message = "Unauthorized access")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                StatusCode = 401
            };
        }

        public static ApiResponse<T> BadRequestResult(string message, List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                Errors = errors ?? new List<string>(),
                StatusCode = 400
            };
        }

        public static ApiResponse<T> ServerErrorResult(string message = "Internal server error")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                StatusCode = 500
            };
        }
    }

    // For responses without data
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public int StatusCode { get; set; }

        public static ApiResponse SuccessResult(string message = "Operation completed successfully")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                StatusCode = 200
            };
        }

        public static ApiResponse ErrorResult(string message, int statusCode = 400, List<string> errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>(),
                StatusCode = statusCode
            };
        }

        public static ApiResponse NotFoundResult(string message = "Resource not found")
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                StatusCode = 404
            };
        }

        public static ApiResponse UnauthorizedResult(string message = "Unauthorized access")
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                StatusCode = 401
            };
        }

        public static ApiResponse BadRequestResult(string message, List<string> errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>(),
                StatusCode = 400
            };
        }

        public static ApiResponse ServerErrorResult(string message = "Internal server error")
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                StatusCode = 500
            };
        }
    }
} 