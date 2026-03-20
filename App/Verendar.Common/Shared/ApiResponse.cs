namespace Verendar.Common.Shared
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public object? Metadata { get; set; }

        public static ApiResponse<T> SuccessResponse(
            T data,
            string message = "Success",
            object? metadata = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = message,
                Data = data,
                Metadata = metadata
            };
        }

        public static ApiResponse<T> CreatedResponse(
            T data,
            string message = "Created")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                StatusCode = 201,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> FailureResponse(
            string message,
            int statusCode = 400,
            object? metadata = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = statusCode,
                Message = message,
                Data = default,
                Metadata = metadata
            };
        }

        public static ApiResponse<T> NotFoundResponse(string message) =>
            FailureResponse(message, 404);

        public static ApiResponse<T> ConflictResponse(string message) =>
            FailureResponse(message, 409);

        public static ApiResponse<T> ForbiddenResponse(string message) =>
            FailureResponse(message, 403);

        public static ApiResponse<List<T>> SuccessPagedResponse<T>(
            List<T> items,
            int totalItems,
            int pageNumber,
            int pageSize,
            string message = "Success")
        {
            var pagingMetadata = new PagingMetadata(totalItems, pageNumber, pageSize);

            return new ApiResponse<List<T>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = message,
                Data = items,
                Metadata = pagingMetadata
            };
        }
    }
}
