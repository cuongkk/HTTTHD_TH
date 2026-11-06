using Microsoft.AspNetCore.Mvc;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

public static class ControllerResults
{
    public static ActionResult<T> ToActionResult<T>(this ControllerBase controller, ServiceResult<T> result)
        where T : class =>
        result.Value is not null
            ? controller.StatusCode(result.StatusCode, result.Value)
            : controller.StatusCode(result.StatusCode, new { code = result.Code, message = result.Message });
}
