using System.Net;

namespace ExpenseTracker.Application.Common.Exceptions;

public class UnauthorizedAppException(string message) : AppException(message, (int)HttpStatusCode.Unauthorized)
{
}

