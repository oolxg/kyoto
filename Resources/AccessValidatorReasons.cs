namespace Smug.Resources;

public class AccessValidatorReasons
{
    public const string IpIsWhitelisted = "IP is whitelisted";
    public const string IpIsBanned = "IP is banned";
    public const string TokenIsBanned = "Token is banned";
    public const string TokenIsWhitelisted = "Token is whitelisted";
    public const string RequestIsFromCrawler = "Request is from a crawler like Yandex or Google bot";
    public const string UserAgentIsEmpty = "User-Agent header is empty";
    public const string BadBotUserAgent = "User-Agent contains `python`, seems like a bot";
    public const string JsRedirReferer = "Referer contains `jsredir`, seems like a RKN bot";
    public const string RequestedUrlIsBlocked = "Requested URL is blocked";

    public const string RequestWasMadeToRecentlyBlockedPage =
        "Request was made to the page that received blocked request less than 5 minutes ago w/ referer";

    public const string RequestWasMadeToRecentlyBlockedPageWithReferer =
        "Request was made to the page that received blocked request less than 30 minutes ago w/o referer";

    public const string PopularPageRequested = "Requested page is popular, so will ignore last blocked request(s)";
    public const string RequestIsValid = "Request is valid";
}