using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Presentation.Abstractions
{
    public sealed record ApiResponse<T>(
        bool IsSuccess,
        T? Data,
        string? ErrorCode,
        string? ErrorMessage,
        DateTime Timestamp)
    {
        public static ApiResponse<T> Success(T data) =>
            new(true, data, null, null, DateTime.UtcNow);

        public static ApiResponse<T> Failure(Error error) =>
            new(false, default, error.Code, error.Message, DateTime.UtcNow);
    }

    // Helper static class
    public static class ApiResponse
    {
        public static ApiResponse<T> Ok<T>(T data) => ApiResponse<T>.Success(data);
        public static ApiResponse<object> Fail(Error error) => ApiResponse<object>.Failure(error);
    }
}
