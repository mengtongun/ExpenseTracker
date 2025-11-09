using System.Net;

namespace ExpenseTracker.Application.Common.Exceptions;

public class NotFoundException(string message) : AppException(message, (int)HttpStatusCode.NotFound)
{
}

