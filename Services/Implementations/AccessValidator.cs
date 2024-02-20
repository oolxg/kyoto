using Smug.Models;
using Smug.Services.Interfaces;

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
        {
            return new AccessValidationResult(false, "IP is whitelisted");
        }
        
        var validationResult = ValidateUserAgent(userRequest.UserAgent);
        if (validationResult.Block)
        {
            return validationResult;
        }
        
        validationResult = ValidateReferer(userRequest.Referer);
        if (validationResult.Block)
        {
            return validationResult;
        }
        
        if (ip.Status == IpStatus.Banned)
        {
            return new AccessValidationResult(true, "IP is banned");
        }
        
        if (userRequest.TokenInfo != null)
        {
            var token = await tokenRepository.FindOrCreateTokenAsync(userRequest.TokenInfo.Token);
            
            switch (token.Status)
            {
                case TokenStatus.Banned:
                    return new AccessValidationResult(true, "Token is banned");
            
                case TokenStatus.Whitelisted:
                    return new AccessValidationResult(false, "Token is whitelisted");
                
                case TokenStatus.Normal:
                    break;
                
                default:
                    throw new Exception("Unknown token status");
            }
        }
        if (IsRequestFromCrawler(userRequest.UserAgent!))
        {
            return new AccessValidationResult(false, "Request is from a crawler like Yandex or Google bot");
        }
        
        if (await IsUrlBlocked(userRequest.Host, userRequest.Path))
        {
            return new AccessValidationResult(true, "Requested URL is blocked");
        }
        
        var blockedRequests = await userRequestRepository.GetBlockedRequestsAsync(userRequest.Host, userRequest.Path);
        
        if (blockedRequests.Count > 0)
        {
            if (await IsRequestedPagePopular(userRequest.Host, userRequest.Path, DateTime.UtcNow.AddHours(-12), 60))
            {
                return new AccessValidationResult(false, "Requested page is popular, so will ignore last blocked request(s)");
            }

            var timePassed = DateTime.UtcNow - blockedRequests.First().RequestDate;

            if (userRequest.Referer == null && timePassed.TotalMinutes < 5 && userRequest.Path != "/")
            {
                return new AccessValidationResult(true, "Request was made to the page that was blocked less than 5 minutes ago. User has no referer.");
            }
        
            if (userRequest.Referer != null && timePassed.TotalMinutes < 30 && userRequest.Path != "/")
            {
                return new AccessValidationResult(true, "Request was made to the page that was blocked less than 30 minutes ago. User has referer.");
            }
        }

        return new AccessValidationResult(false, "Request is valid");
    }
    
    private static AccessValidationResult ValidateUserAgent(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return new AccessValidationResult(true, "User-Agent is empty");
        }
        
        if (userAgent.ToLower().Contains("python"))
        {
            return new AccessValidationResult(true, "User-Agent contains `python`, seems like a bot");
        }
        
        return new AccessValidationResult(false, "User agent is valid");
    }
    
    private static AccessValidationResult ValidateReferer(string? referer)
    {
        if (!string.IsNullOrEmpty(referer) && referer.Contains("yandex.ru/clck/jsredir"))
        {
            return new AccessValidationResult(true, "Referer contains `jsredir`, seems like a bot");
        }
        
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