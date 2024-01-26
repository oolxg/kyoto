namespace Smug.Utils;

public class TokenRepositoryException : Exception
{
    public TokenRepositoryException()
    {
    }

    public TokenRepositoryException(string message) : base(message)
    {
    }

    public TokenRepositoryException(string message, Exception inner) : base(message, inner)
    {
    }
}