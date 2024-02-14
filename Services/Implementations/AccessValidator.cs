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
        var validationResult = new AccessValidationResult
        {
            block = false
        };
        
        var ip = await ipRepository.FindOrCreateIpAsync(userRequest.IpInfo.Ip);
        if (ip.Status == IpAddressInfo.IpStatus.Whitelisted)
        {
            return validationResult;
        }
        
        validationResult = ValidateUserAgent(userRequest.UserAgent);
        if (validationResult.block)
        {
            return validationResult;
        }
        
        if (ip.Status == IpAddressInfo.IpStatus.Banned)
        {
            return new AccessValidationResult
            {
                block = true,
                reason = "IP is banned"
            };
        }
        
        if (userRequest.TokenInfo != null)
        {
            var token = await tokenRepository.FindOrCreateTokenAsync(userRequest.TokenInfo.Token);

            switch (token.Status)
            {
                case TokenInfo.TokenStatus.Banned:
                    return new AccessValidationResult
                    {
                        block = true,
                        reason = "Token is banned"
                    };
            
                case TokenInfo.TokenStatus.Whitelisted:
                    return validationResult;
                
                case TokenInfo.TokenStatus.Normal:
                    break;
                
                default:
                    throw new Exception("Unknown token status");
            }
        }
        
        if (userRequest.TokenInfo?.Status == TokenInfo.TokenStatus.Banned)
        {
            return new AccessValidationResult
            {
                block = true,
                reason = "Token is banned"
            };
        }
        
        if (IsRequestFromCrawler(userRequest.UserAgent!))
        {
            return new AccessValidationResult
            {
                block = false
            };
        }
        
        if (await IsUrlInBlocked(userRequest.Host, userRequest.Path))
        {
            return new AccessValidationResult
            {
                block = true,
                reason = "URL is blocked"
            };
        }
        
        var blockedRequests = await userRequestRepository.GetBlockedRequestsAsync(userRequest.Host, userRequest.Path);
        
        if (blockedRequests.Count == 0)
        {
            return new AccessValidationResult
            {
                block = false
            };
        }

        if (await IsRequestedPagePopular(userRequest.Host, userRequest.Path, DateTime.UtcNow.AddHours(-12), 60))
        {
            return new AccessValidationResult
            {
                block = false
            };
        }

        var timePassed = DateTime.UtcNow - blockedRequests.First().RequestDate;

        if (userRequest.Referer == null && timePassed.TotalMinutes < 5 && userRequest.Path != "/")
        {
            validationResult.block = true;
            validationResult.reason = "Request was made to the page that was blocked less than 5 minutes ago. User has no referer.";
        }
        else if (userRequest.Referer != null && timePassed.TotalMinutes < 30 && userRequest.Path != "/")
        {
            validationResult.block = true;
            validationResult.reason = "Request was made to the page that was blocked less than 30 minutes ago. User has referer.";
        }

        return validationResult;
    }
    
    private static AccessValidationResult ValidateUserAgent(string? userAgent)
    {
        var result = new AccessValidationResult
        {
            block = false
        };
        
        if (string.IsNullOrEmpty(userAgent))
        {
            result.block = true;
            result.reason = "User agent is empty";
            return result;
        }
        
        if (userAgent.Contains("python"))
        {
            result.block = true;
            result.reason = "User agent contains `python`, seems like a bot";
            return result;
        }
        
        if (userAgent.Contains("yandex.ru/clck/jsredir"))
        {
            result.block = true;
            result.reason = "User agent contains `yandex.ru/clck/jsredir`, seems like a RKN bot";
            return result;
        }
     
        return result;
    }
    
    private static bool IsRequestFromCrawler(string userAgent)
    {
        return userAgent.Contains("bot") || userAgent.Contains("crawler");
    }
    
    private async Task<bool> IsUrlInBlocked(string host, string path)
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