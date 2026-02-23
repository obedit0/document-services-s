namespace Domain.Exceptions;

public abstract class HttpErrorException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    protected HttpErrorException(int statusCode, string errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            throw new ArgumentException("Error code cannot be empty.", nameof(errorCode));

        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

public sealed class ClientErrorException : HttpErrorException
{
    public ClientErrorException(int statusCode, string errorCode, string message, Exception? innerException = null)
        : base(statusCode, errorCode, message, innerException)
    {
        if (statusCode < 400 || statusCode > 499)
            throw new ArgumentOutOfRangeException(nameof(statusCode), "Client error status code must be between 400 and 499.");
    }
}

public sealed class ServerErrorException : HttpErrorException
{
    public ServerErrorException(int statusCode, string errorCode, string message, Exception? innerException = null)
        : base(statusCode, errorCode, message, innerException)
    {
        if (statusCode < 500 || statusCode > 599)
            throw new ArgumentOutOfRangeException(nameof(statusCode), "Server error status code must be between 500 and 599.");
    }
}
