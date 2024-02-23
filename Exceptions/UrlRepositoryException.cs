namespace Kyoto.Exceptions;

public class UrlRepositoryException : Exception
{
    public UrlRepositoryException(string message) : base(message)
    {
    }

    public UrlRepositoryException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public UrlRepositoryException()
    {
    }
}