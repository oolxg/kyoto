using Kyoto.Models;

namespace Kyoto.Services.Interfaces;

public interface IAccessValidator
{
    /// <summary>
    /// Validates the user request and returns the validation result.
    /// </summary>
    /// <note>
    /// If the IP or token is whitelisted, but token or IP(respectively) is banned, the validation result will be { Block: false, Reason: "{IP|Token} is whitelisted}" }
    /// </note>
    /// <param name="userRequest">Request information.</param>
    /// <returns>Validation result.</returns>
    Task<AccessValidationResult> ValidateAsync(UserRequest userRequest);
}