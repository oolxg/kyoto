using Kyoto.Models;

namespace Kyoto.Services.Interfaces;

public interface IAccessValidator
{
    /// <summary>
    /// Validates the user request and returns the validation result.
    /// </summary>
    /// <param name="userRequest">Request information.</param>
    /// <returns>Validation result.</returns>
    Task<AccessValidationResult> ValidateAsync(UserRequest userRequest);
}