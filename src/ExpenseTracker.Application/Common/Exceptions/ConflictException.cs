using System.Net;

namespace ExpenseTracker.Application.Common.Exceptions;

public class ConflictException(string message) : AppException(message, (int)HttpStatusCode.Conflict)
{
}

