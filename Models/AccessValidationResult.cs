namespace Smug.Models;

public class AccessValidationResult(bool block, string reason)
{
    public bool Block { get; } = block;
    public string Reason { get; } = reason;
}