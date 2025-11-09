using ExpenseTrackerApi.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTrackerApi.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected int CurrentUserId => User.GetUserId();
}

