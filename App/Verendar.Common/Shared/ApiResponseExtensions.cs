using Microsoft.AspNetCore.Http;

namespace Verendar.Common.Shared
{
    public static class ApiResponseExtensions
    {
        /// <summary>
        /// Converts an ApiResponse to the correct IResult based on its StatusCode.
        /// Eliminates the repeated "IsSuccess ? Results.Ok : Results.BadRequest" pattern in handlers.
        /// </summary>
        public static IResult ToHttpResult<T>(this ApiResponse<T> response) => response.StatusCode switch
        {
            200 => Results.Ok(response),
            201 => Results.Json(response, statusCode: 201),
            400 => Results.BadRequest(response),
            403 => Results.Json(response, statusCode: StatusCodes.Status403Forbidden),
            404 => Results.NotFound(response),
            409 => Results.Conflict(response),
            _ => Results.Json(response, statusCode: response.StatusCode)
        };
    }
}
