namespace Smug.Exceptions;

public class UserRequestRepositoryException : Exception
{
    public UserRequestRepositoryException(string message) : base(message)
    {
    }

    public UserRequestRepositoryException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public UserRequestRepositoryException()
    {
    }
}