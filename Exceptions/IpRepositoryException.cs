namespace Smug.Utils;

public class IpRepositoryException : Exception
{
    public IpRepositoryException()
    {
    }
    
    public IpRepositoryException(string message) : base(message)
    {
    }
    
    public IpRepositoryException(string message, Exception inner) : base(message, inner)
    {
    }
}