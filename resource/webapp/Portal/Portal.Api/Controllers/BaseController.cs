using System.Security.Claims;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Portal.Core.Services;
using Portal.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;

namespace Portal.Api.Controllers;

public class BaseController : Controller
{
    private readonly ILogger _logger_;
    public BaseController(ILogger logger)
    {
        _logger_ = logger;

    }
    protected async Task<Result<PersonDto>> TryCreatePersonFromAadUser(
        IPersonService personService,
        ClaimsPrincipal user)
    {
        try
        {
            var aadId = ExtractAadId(user);
            var email = user.Claims.Where(c => c.Type.Contains("identity/claims/name")).Select(c => c.Value).First();
            var personAadInfo = new PersonAadInfo()
            {
                Id = aadId,
                Email = email,
                FullName = user.Claims.Where(c => c.Type == "name").Select(c => c.Value).First()
            };
            return await personService.TryCreatePersonAsync(personAadInfo);
        }
        catch (Exception e)
        {
            _logger_.LogError(e, $"Exception on {nameof(TryCreatePersonFromAadUser)}");
            return new Result<PersonDto>(e);
        }
    }

    protected static Guid ExtractAadId(ClaimsPrincipal user)
    {
        var aadId = user.Claims.Where(c => c.Type.Contains("identity/claims/objectidentifier")).Select(c => c.Value).First();
        return new Guid(aadId);
    }

    public IActionResult HandleResult<T>(Result<T> result)
    {
        var resultHandled = result.Match<IActionResult>(
            succ => Ok(result), exception =>
            {
                return BadRequest(new ExceptionDetails(exception));
            });
        return resultHandled;
    }

    private class ExceptionDetails
    {
        public String Message { get; set; }
        public String? StackTrace { get; set; }
        public ExceptionDetails(Exception e)
        {
            Message = e.Message;
            StackTrace = e.StackTrace;
        }
    }
}