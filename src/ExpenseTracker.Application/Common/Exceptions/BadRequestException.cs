using System.Net;

namespace ExpenseTracker.Application.Common.Exceptions;

public class BadRequestException(string message) : AppException(message, (int)HttpStatusCode.BadRequest)
{
}

