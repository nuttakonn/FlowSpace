using Microsoft.AspNetCore.Mvc;
using FlowSpace.Application.Common.Models;
using FlowSpace.Application.Common.Behaviors;

namespace FlowSpace.Api.Controllers;

[ApiController]
public abstract class ApiController : ControllerBase
{
    protected IActionResult HandleFailure(Result result) =>
        result switch
        {
            { IsSuccess: true } => throw new InvalidOperationException(),
            IValidationResult validationResult =>
                BadRequest(
                    CreateProblemDetails(
                        "Validation Error", 
                        StatusCodes.Status400BadRequest,
                        result.Error,
                        validationResult.Errors)),
            _ when result.Error.Code.EndsWith(".NotFound") || result.Error.Code == "NotFound" =>
                NotFound(
                    CreateProblemDetails(
                        "Not Found",
                        StatusCodes.Status404NotFound,
                        result.Error)),
            _ =>
                BadRequest(
                    CreateProblemDetails(
                        "Bad Request",
                        StatusCodes.Status400BadRequest,
                        result.Error))
        };

    private static ProblemDetails CreateProblemDetails(
        string title,
        int status,
        Error error,
        Error[]? errors = null) =>
        new()
        {
            Title = title,
            Type = error.Code,
            Detail = error.Description,
            Status = status,
            Extensions = { { nameof(errors), errors } }
        };
}
