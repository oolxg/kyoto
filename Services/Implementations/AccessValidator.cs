using Smug.Models;
using Smug.Services.Interfaces;

namespace Smug.Services.Implementations;

public class AccessValidator : IAccessValidator
{
    private readonly IUserRequestRepository _userRequestRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly IIpRepository _ipRepository;
    
    public AccessValidator(
        IUserRequestRepository userRequestRepository,
        ITokenRepository tokenRepository,
        IIpRepository ipRepository)
    {
        _userRequestRepository = userRequestRepository;
        _tokenRepository = tokenRepository;
        _ipRepository = ipRepository;
    }
    
    public async Task<AccessValidationResult> ValidateAsync(UserRequest userRequest)
    {
        var ip = await _ipRepository.FindOrCreateIpAsync(userRequest.IpInfo.Ip);
        if (ip.Status == IpAddressInfo.IpStatus.Whitelisted)
        {
            return new AccessValidationResult
            {
                block = false
            };
        }
        
        if (userRequest.UserAgent == null)
        {
            return new AccessValidationResult
            {
                block = true,
                reason = "User agent is not specified."
            };
        }
        
        if (userRequest.Referer?.Contains("yandex.ru/clck/jsredir") == true)
        {
            return new AccessValidationResult
            {
                block = true,
                reason = "User has in referer `jsredir`, seems like a RKN bot"
            };
        }
        
        if (ip.Status == IpAddressInfo.IpStatus.Banned)
        {
            return new AccessValidationResult
            {
                block = true,
                reason = "IP is banned"
            };
        }
        
        if (userRequest.TokenInfo?.Status == TokenInfo.TokenStatus.Banned)
        {
            return new AccessValidationResult
            {
                block = true,
                reason = "Token is banned"
            };
        }
        
        if (IsRequestFromCrawler(userRequest.UserAgent))
        {
            return new AccessValidationResult
            {
                block = true,
                reason = "Request is from a crawler"
            };
        }
        
        

        return new AccessValidationResult
        {
            block = true
        };
    }
    
    private static bool IsRequestFromCrawler(string userAgent)
    {
        return userAgent.Contains("bot") || userAgent.Contains("crawler");
    }
    
    private async Task<bool> IsUrlInBlocked(string host, string path)
    {
        var requestedPath = path.EndsWith('/') ? path : path + '/';
        
        // var foundBannedUrl = await
        return false;
    }
}