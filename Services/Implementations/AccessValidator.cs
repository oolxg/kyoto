using Smug.Models;
using Smug.Services.Interfaces;
using Smug.Resources;

namespace Smug.Services.Implementations;

public class AccessValidator(
    IUserRequestRepository userRequestRepository,
    ITokenRepository tokenRepository,
    IIpRepository ipRepository,
    IRestrictedUrlRepository restrictedUrlRepository) : IAccessValidator
{
    public async Task<AccessValidationResult> ValidateAsync(UserRequest userRequest)
    {
        var ip = await ipRepository.FindOrCreateIpAsync(userRequest.IpInfo.Ip);
        if (ip.Status == IpStatus.Whitelisted)
            return new AccessValidationResult(false, AccessValidatorReasons.IpIsWhitelisted);

        var validationResult = ValidateUserAgent(userRequest.UserAgent);
        if (validationResult.Block) return validationResult;

        validationResult = ValidateReferer(userRequest.Referer);
        if (validationResult.Block) return validationResult;

        if (ip.Status == IpStatus.Banned) return new AccessValidationResult(true, AccessValidatorReasons.IpIsBanned);

        if (userRequest.TokenInfo != null)
        {
            var token = await tokenRepository.FindOrCreateTokenAsync(userRequest.TokenInfo.Token);

            switch (token.Status)
            {
                case TokenStatus.Banned:
                    return new AccessValidationResult(true, AccessValidatorReasons.TokenIsBanned);

                case TokenStatus.Whitelisted:
                    return new AccessValidationResult(false, AccessValidatorReasons.TokenIsWhitelisted);

                case TokenStatus.Normal:
                    break;

                default:
                    throw new Exception("Unknown token status");
            }
        }

        if (IsRequestFromCrawler(userRequest.UserAgent!))
            return new AccessValidationResult(false, AccessValidatorReasons.RequestIsFromCrawler);

        if (await IsUrlBlocked(userRequest.Host, userRequest.Path))
            return new AccessValidationResult(true, AccessValidatorReasons.RequestedUrlIsBlocked);

        var timeThreshold = DateTime.UtcNow.AddMinutes(userRequest.Referer == null ? -30 : -5);
        var blockedRequests = await userRequestRepository
            .GetBlockedRequestsAsync(userRequest.Host, userRequest.Path, timeThreshold);

        if (blockedRequests.Count == 0) return new AccessValidationResult(false, AccessValidatorReasons.RequestIsValid);

        if (await IsRequestedPagePopular(userRequest.Host, userRequest.Path, DateTime.UtcNow.AddHours(-12), 60))
            return new AccessValidationResult(false, AccessValidatorReasons.PopularPageRequested);

        return new AccessValidationResult(true, userRequest.Referer == null
            ? AccessValidatorReasons.RequestWasMadeToRecentlyBlockedPage
            : AccessValidatorReasons.RequestWasMadeToRecentlyBlockedPageWithReferer);
    }

    private static AccessValidationResult ValidateUserAgent(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return new AccessValidationResult(true, AccessValidatorReasons.UserAgentIsEmpty);

        if (userAgent.ToLower().Contains("python"))
            return new AccessValidationResult(true, AccessValidatorReasons.BadBotUserAgent);

        return new AccessValidationResult(false, "User-Agent is valid");
    }

    private static AccessValidationResult ValidateReferer(string? referer)
    {
        if (!string.IsNullOrEmpty(referer) && referer.Contains("yandex.ru/clck/jsredir"))
            return new AccessValidationResult(true, AccessValidatorReasons.JsRedirReferer);

        return new AccessValidationResult(false, "Referer is valid");
    }

    private static bool IsRequestFromCrawler(string userAgent)
    {
        return userAgent.ToLower().Contains("bot") || userAgent.ToLower().Contains("crawler");
    }

    private async Task<bool> IsUrlBlocked(string host, string path)
    {
        var requestedPath = path.EndsWith('/') ? path : path + '/';

        return await restrictedUrlRepository.IsUrlBlocked(host, requestedPath);
    }

    private async Task<bool> IsRequestedPagePopular(string host, string path, DateTime start, int threshold)
    {
        var requests = await userRequestRepository.GetUserRequestsOnEndPointsAsync(host, path, start);

        return requests.Count > threshold;
    }
}