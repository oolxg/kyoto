namespace Smug.Exceptions;

public class RestrictedUrlRepositoryException : Exception
{
    public RestrictedUrlRepositoryException(string message) : base(message)
    {
    }

    public RestrictedUrlRepositoryException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public RestrictedUrlRepositoryException()
    {
    }
}