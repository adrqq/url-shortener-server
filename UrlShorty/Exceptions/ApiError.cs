using System;

namespace UrlShorty.Exceptions
{

    public class ApiError : Exception
    {
        public int StatusCode { get; }

        public ApiError(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public ApiError(int statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public static ApiError UserNotActivated()
        {
            return new ApiError(401, "User not activated");
        }
    }
}
