using Emp.Api.Controllers;
using Emp.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Emp.XUnitTests.Helpers;

public static class Helpers
{
    public static bool CheckForLogMessage(object value, string expectedLogMessage)
    {
        return value?.ToString() is string message
            && message.Contains(expectedLogMessage);
    }

    public static void ApplyModelStateErrors(CreateUserDto dto, BaseController controller)
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, context, results, true);

        foreach (var error in results)
            controller.ModelState.AddModelError(error.MemberNames.First(), error.ErrorMessage);
    }
}
