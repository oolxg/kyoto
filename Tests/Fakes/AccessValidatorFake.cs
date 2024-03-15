using Kyoto.Models;
using Kyoto.Services.Interfaces;

namespace Kyoto.Tests.Fakes;

public class AccessValidatorFake : IAccessValidator
{
    public int ValidateAsyncCount { get; private set; } = 0;
    public AccessValidationResult AccessValidationResultToReturn { get; set; }
    public Task<AccessValidationResult> ValidateAsync(UserRequest userRequest)
    {
        ValidateAsyncCount++;
        return Task.FromResult(AccessValidationResultToReturn);
    }
}